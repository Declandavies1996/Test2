using CaeDashboard.EngineeringRequests.Dtos;
using CaeDashboard.EngineeringRequests.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CaeDashboard.EngineeringRequests.Services;

public class EngineeringRequestService<TDbContext> where TDbContext : DbContext
{
    private readonly TDbContext _db;
    private readonly EngineeringRequestUploadOptions _uploadOptions;

    public EngineeringRequestService(TDbContext db, IOptions<EngineeringRequestUploadOptions> uploadOptions)
    {
        _db = db;
        _uploadOptions = uploadOptions.Value;
    }

    private DbSet<EngineeringRequest> Requests => _db.Set<EngineeringRequest>();
    private DbSet<RequestNote> RequestNotes => _db.Set<RequestNote>();
    private DbSet<RequestAttachment> RequestAttachments => _db.Set<RequestAttachment>();
    private DbSet<RequestHistory> RequestHistory => _db.Set<RequestHistory>();
    private DbSet<RequestRunbook> RequestRunbooks => _db.Set<RequestRunbook>();
    private DbSet<Runbook> Runbooks => _db.Set<Runbook>();
    private DbSet<EngineeringSystem> Systems => _db.Set<EngineeringSystem>();
    private DbSet<RecurringIssue> RecurringIssues => _db.Set<RecurringIssue>();

    public async Task<IReadOnlyList<EngineeringRequestListItemDto>> GetRequestsAsync(
        string? search,
        RequestStatus? status,
        string? system,
        string? currentUserName,
        bool myRequestsOnly,
        CancellationToken cancellationToken)
    {
        var query = Requests.AsNoTracking();

        if (myRequestsOnly && !string.IsNullOrWhiteSpace(currentUserName))
        {
            query = query.Where(x => x.OwnerUserName == currentUserName || x.CreatedByUserName == currentUserName || x.SubmittedByUserName == currentUserName);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.Title.Contains(term) ||
                (x.Description != null && x.Description.Contains(term)) ||
                (x.RequestedBy != null && x.RequestedBy.Contains(term)) ||
                (x.Department != null && x.Department.Contains(term)));
        }

        if (status is not null)
        {
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(system))
        {
            query = query.Where(x => x.SystemName == system);
        }

        return await query
            .OrderBy(x => x.Status == RequestStatus.Done)
            .ThenBy(x => x.Priority)
            .ThenByDescending(x => x.UpdatedDate)
            .Select(x => new EngineeringRequestListItemDto(
                x.Id,
                x.Title,
                x.SystemName,
                x.RequestedBy,
                x.Department,
                x.Priority,
                x.Status,
                x.Type,
                x.CreatedDate,
                x.UpdatedDate,
                x.OwnerUserName,
                x.SubmittedByUserName,
                x.IsUserSubmitted,
                x.RequiresTriage))
            .ToListAsync(cancellationToken);
    }

    public async Task<EngineeringRequestDetailDto?> GetRequestAsync(int id, CancellationToken cancellationToken)
    {
        return await Requests
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new EngineeringRequestDetailDto(
                x.Id,
                x.Title,
                x.Description,
                x.SystemName,
                x.RequestedBy,
                x.Department,
                x.Priority,
                x.Status,
                x.Type,
                x.CreatedDate,
                x.UpdatedDate,
                x.Notes,
                x.OwnerUserName,
                x.CreatedByUserName,
                x.UpdatedByUserName,
                x.SubmittedByUserName,
                x.TriagedByUserName,
                x.TriagedDate,
                x.IsUserSubmitted,
                x.RequiresTriage,
                x.UrgencyExplanation,
                x.BusinessReason,
                x.ExpectedBehaviour,
                x.ActualBehaviour,
                x.RequestNotes
                    .OrderByDescending(n => n.CreatedDate)
                    .Select(n => new RequestNoteDto(n.Id, n.RequestId, n.NoteText, n.CreatedBy, n.CreatedDate))
                    .ToList(),
                x.Attachments
                    .OrderByDescending(a => a.UploadedDate)
                    .Select(a => new RequestAttachmentDto(
                        a.Id,
                        a.RequestId,
                        a.FileName,
                        a.StoredFileName,
                        a.FilePath,
                        a.ContentType,
                        a.UploadedBy,
                        a.UploadedDate))
                    .ToList(),
                x.History
                    .OrderByDescending(h => h.ChangedDate)
                    .Select(h => new RequestHistoryDto(
                        h.Id,
                        h.RequestId,
                        h.ActionType,
                        h.OldValue,
                        h.NewValue,
                        h.ChangedBy,
                        h.ChangedDate))
                    .ToList(),
                x.RequestRunbooks
                    .OrderByDescending(r => r.LinkedDate)
                    .Select(r => new LinkedRunbookDto(
                        r.Id,
                        r.RunbookId,
                        r.Runbook != null ? r.Runbook.Title : string.Empty,
                        r.Runbook != null ? r.Runbook.SystemName : string.Empty,
                        r.Runbook != null ? r.Runbook.Category : RunbookCategory.Other,
                        r.LinkedDate))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CreateRequestAsync(UpsertEngineeringRequestDto dto, string currentUserName, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var request = new EngineeringRequest
        {
            Title = dto.Title.Trim(),
            Description = dto.Description,
            SystemName = dto.SystemName.Trim(),
            RequestedBy = dto.RequestedBy,
            Department = dto.Department,
            Priority = dto.Priority,
            Status = dto.Status,
            Type = dto.Type,
            Notes = dto.Notes,
            OwnerUserName = string.IsNullOrWhiteSpace(dto.OwnerUserName) ? currentUserName : dto.OwnerUserName,
            CreatedByUserName = currentUserName,
            UpdatedByUserName = currentUserName,
            BusinessReason = dto.BusinessReason,
            ExpectedBehaviour = dto.ExpectedBehaviour,
            ActualBehaviour = dto.ActualBehaviour,
            UrgencyExplanation = dto.UrgencyExplanation,
            CreatedDate = now,
            UpdatedDate = now
        };

        Requests.Add(request);
        await _db.SaveChangesAsync(cancellationToken);

        RequestHistory.Add(new RequestHistory
        {
            RequestId = request.Id,
            ActionType = "RequestCreated",
            NewValue = request.Title,
            ChangedBy = currentUserName,
            ChangedDate = now
        });
        await _db.SaveChangesAsync(cancellationToken);

        return request.Id;
    }

    public async Task<bool> UpdateRequestAsync(int id, UpsertEngineeringRequestDto dto, string currentUserName, CancellationToken cancellationToken)
    {
        var request = await Requests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (request is null)
        {
            return false;
        }

        var oldStatus = request.Status;
        var oldPriority = request.Priority;
        var oldSystem = request.SystemName;
        var oldOwner = request.OwnerUserName;
        var oldType = request.Type;

        request.Title = dto.Title.Trim();
        request.Description = dto.Description;
        request.SystemName = dto.SystemName.Trim();
        request.RequestedBy = dto.RequestedBy;
        request.Department = dto.Department;
        request.Priority = dto.Priority;
        request.Status = dto.Status;
        request.Type = dto.Type;
        request.Notes = dto.Notes;
        request.OwnerUserName = string.IsNullOrWhiteSpace(dto.OwnerUserName) ? request.OwnerUserName : dto.OwnerUserName;
        request.BusinessReason = dto.BusinessReason;
        request.ExpectedBehaviour = dto.ExpectedBehaviour;
        request.ActualBehaviour = dto.ActualBehaviour;
        request.UrgencyExplanation = dto.UrgencyExplanation;
        request.UpdatedByUserName = currentUserName;
        request.UpdatedDate = DateTime.UtcNow;

        if (oldStatus != request.Status)
        {
            RequestHistory.Add(new RequestHistory
            {
                RequestId = request.Id,
                ActionType = "StatusChanged",
                OldValue = oldStatus.ToString(),
                NewValue = request.Status.ToString(),
                ChangedBy = currentUserName,
                ChangedDate = request.UpdatedDate
            });
        }

        if (oldPriority != request.Priority)
        {
            RequestHistory.Add(new RequestHistory
            {
                RequestId = request.Id,
                ActionType = "PriorityChanged",
                OldValue = oldPriority.ToString(),
                NewValue = request.Priority.ToString(),
                ChangedBy = currentUserName,
                ChangedDate = request.UpdatedDate
            });
        }

        if (oldSystem != request.SystemName)
        {
            RequestHistory.Add(new RequestHistory
            {
                RequestId = request.Id,
                ActionType = "SystemChanged",
                OldValue = oldSystem,
                NewValue = request.SystemName,
                ChangedBy = currentUserName,
                ChangedDate = request.UpdatedDate
            });
        }

        if (oldType != request.Type)
        {
            RequestHistory.Add(new RequestHistory
            {
                RequestId = request.Id,
                ActionType = "TypeChanged",
                OldValue = oldType.ToString(),
                NewValue = request.Type.ToString(),
                ChangedBy = currentUserName,
                ChangedDate = request.UpdatedDate
            });
        }

        if (oldOwner != request.OwnerUserName)
        {
            RequestHistory.Add(new RequestHistory
            {
                RequestId = request.Id,
                ActionType = "OwnerChanged",
                OldValue = oldOwner,
                NewValue = request.OwnerUserName,
                ChangedBy = currentUserName,
                ChangedDate = request.UpdatedDate
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteRequestAsync(int id, CancellationToken cancellationToken)
    {
        var request = await Requests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (request is null)
        {
            return false;
        }

        Requests.Remove(request);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<RequestNoteDto?> AddNoteAsync(int requestId, AddRequestNoteDto dto, string currentUserName, CancellationToken cancellationToken)
    {
        var request = await Requests.FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);
        if (request is null)
        {
            return null;
        }

        var note = new RequestNote
        {
            RequestId = requestId,
            NoteText = dto.NoteText.Trim(),
            CreatedBy = currentUserName,
            CreatedDate = DateTime.UtcNow
        };

        request.UpdatedDate = DateTime.UtcNow;
        request.UpdatedByUserName = currentUserName;
        RequestNotes.Add(note);
        RequestHistory.Add(new RequestHistory
        {
            RequestId = requestId,
            ActionType = "NoteAdded",
            NewValue = note.NoteText.Length > 250 ? note.NoteText[..250] : note.NoteText,
            ChangedBy = currentUserName,
            ChangedDate = note.CreatedDate
        });
        await _db.SaveChangesAsync(cancellationToken);

        return new RequestNoteDto(note.Id, note.RequestId, note.NoteText, note.CreatedBy, note.CreatedDate);
    }

    public async Task<RequestAttachmentDto?> AddAttachmentAsync(
        int requestId,
        IFormFile file,
        string? uploadedBy,
        CancellationToken cancellationToken)
    {
        var request = await Requests.FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);
        if (request is null)
        {
            return null;
        }

        if (file.Length == 0 || file.Length > _uploadOptions.MaxFileSizeBytes)
        {
            throw new InvalidOperationException("File is empty or exceeds the allowed upload size.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_uploadOptions.AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("This file type is not allowed.");
        }

        var requestFolder = Path.Combine(_uploadOptions.RootPath, requestId.ToString());
        Directory.CreateDirectory(requestFolder);

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var storedPath = Path.Combine(requestFolder, storedFileName);

        await using (var stream = File.Create(storedPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var now = DateTime.UtcNow;
        var attachment = new RequestAttachment
        {
            RequestId = requestId,
            FileName = Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            FilePath = storedPath,
            ContentType = file.ContentType,
            UploadedBy = uploadedBy,
            UploadedDate = now
        };

        request.UpdatedDate = now;
        request.UpdatedByUserName = uploadedBy;
        RequestAttachments.Add(attachment);
        RequestHistory.Add(new RequestHistory
        {
            RequestId = requestId,
            ActionType = "AttachmentUploaded",
            NewValue = attachment.FileName,
            ChangedBy = uploadedBy,
            ChangedDate = now
        });
        await _db.SaveChangesAsync(cancellationToken);

        return new RequestAttachmentDto(
            attachment.Id,
            attachment.RequestId,
            attachment.FileName,
            attachment.StoredFileName,
            attachment.FilePath,
            attachment.ContentType,
            attachment.UploadedBy,
            attachment.UploadedDate);
    }

    public async Task<RequestAttachment?> GetAttachmentEntityAsync(int id, CancellationToken cancellationToken)
    {
        return await RequestAttachments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<SubmitEngineeringRequestResultDto> SubmitRequestAsync(
        SubmitEngineeringRequestDto dto,
        IReadOnlyList<IFormFile> files,
        string currentUserName,
        string? defaultOwnerUserName,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var request = new EngineeringRequest
        {
            Title = dto.Title.Trim(),
            Description = dto.Description,
            BusinessReason = dto.BusinessReason,
            ExpectedBehaviour = dto.ExpectedBehaviour,
            ActualBehaviour = dto.ActualBehaviour,
            SystemName = string.IsNullOrWhiteSpace(dto.SuggestedSystemName) ? "Unassigned" : dto.SuggestedSystemName.Trim(),
            UrgencyExplanation = dto.UrgencyExplanation,
            RequestedBy = currentUserName,
            Department = dto.Department,
            Priority = RequestPriority.P3,
            Status = RequestStatus.Incoming,
            Type = RequestType.Investigation,
            OwnerUserName = string.IsNullOrWhiteSpace(defaultOwnerUserName) ? currentUserName : defaultOwnerUserName,
            CreatedByUserName = currentUserName,
            UpdatedByUserName = currentUserName,
            SubmittedByUserName = currentUserName,
            IsUserSubmitted = true,
            RequiresTriage = true,
            CreatedDate = now,
            UpdatedDate = now
        };

        Requests.Add(request);
        await _db.SaveChangesAsync(cancellationToken);

        RequestHistory.Add(new RequestHistory
        {
            RequestId = request.Id,
            ActionType = "UserSubmitted",
            NewValue = request.Title,
            ChangedBy = currentUserName,
            ChangedDate = now
        });
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var file in files.Where(x => x.Length > 0))
        {
            await AddAttachmentAsync(request.Id, file, currentUserName, cancellationToken);
        }

        return new SubmitEngineeringRequestResultDto(request.Id, request.Title, request.CreatedDate, currentUserName);
    }

    public async Task<IReadOnlyList<EngineeringRequestListItemDto>> GetMySubmittedRequestsAsync(
        string currentUserName,
        CancellationToken cancellationToken)
    {
        return await Requests
            .AsNoTracking()
            .Where(x => x.SubmittedByUserName == currentUserName || x.CreatedByUserName == currentUserName)
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new EngineeringRequestListItemDto(
                x.Id,
                x.Title,
                x.SystemName,
                x.RequestedBy,
                x.Department,
                x.Priority,
                x.Status,
                x.Type,
                x.CreatedDate,
                x.UpdatedDate,
                x.OwnerUserName,
                x.SubmittedByUserName,
                x.IsUserSubmitted,
                x.RequiresTriage))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EngineeringRequestListItemDto>> GetTriageRequestsAsync(
        string currentUserName,
        CancellationToken cancellationToken)
    {
        return await Requests
            .AsNoTracking()
            .Where(x => x.RequiresTriage && x.Status == RequestStatus.Incoming && x.OwnerUserName == currentUserName)
            .OrderBy(x => x.CreatedDate)
            .Select(x => new EngineeringRequestListItemDto(
                x.Id,
                x.Title,
                x.SystemName,
                x.RequestedBy,
                x.Department,
                x.Priority,
                x.Status,
                x.Type,
                x.CreatedDate,
                x.UpdatedDate,
                x.OwnerUserName,
                x.SubmittedByUserName,
                x.IsUserSubmitted,
                x.RequiresTriage))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> TriageRequestAsync(
        int id,
        TriageEngineeringRequestDto dto,
        string currentUserName,
        CancellationToken cancellationToken)
    {
        var request = await Requests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (request is null)
        {
            return false;
        }

        var now = DateTime.UtcNow;
        var oldSystem = request.SystemName;
        var oldPriority = request.Priority;
        var oldStatus = request.Status;
        var oldType = request.Type;
        var oldOwner = request.OwnerUserName;

        request.SystemName = dto.SystemName.Trim();
        request.Priority = dto.Priority;
        request.Status = dto.Status;
        request.Type = dto.Type;
        request.OwnerUserName = string.IsNullOrWhiteSpace(dto.OwnerUserName) ? currentUserName : dto.OwnerUserName;
        request.Notes = dto.Notes;
        request.RequiresTriage = false;
        request.TriagedByUserName = currentUserName;
        request.TriagedDate = now;
        request.UpdatedByUserName = currentUserName;
        request.UpdatedDate = now;

        RequestHistory.Add(new RequestHistory { RequestId = id, ActionType = "RequestTriaged", ChangedBy = currentUserName, ChangedDate = now });
        AddHistoryIfChanged(id, "SystemAssigned", oldSystem, request.SystemName, currentUserName, now);
        AddHistoryIfChanged(id, "PriorityAssigned", oldPriority.ToString(), request.Priority.ToString(), currentUserName, now);
        AddHistoryIfChanged(id, "StatusChanged", oldStatus.ToString(), request.Status.ToString(), currentUserName, now);
        AddHistoryIfChanged(id, "TypeAssigned", oldType.ToString(), request.Type.ToString(), currentUserName, now);
        AddHistoryIfChanged(id, "OwnerAssigned", oldOwner, request.OwnerUserName, currentUserName, now);

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private void AddHistoryIfChanged(int requestId, string actionType, string? oldValue, string? newValue, string currentUserName, DateTime changedDate)
    {
        if (oldValue == newValue)
        {
            return;
        }

        RequestHistory.Add(new RequestHistory
        {
            RequestId = requestId,
            ActionType = actionType,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedBy = currentUserName,
            ChangedDate = changedDate
        });
    }

    public async Task<LinkedRunbookDto?> LinkRunbookAsync(
        int requestId,
        int runbookId,
        string? changedBy,
        CancellationToken cancellationToken)
    {
        var request = await Requests.FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);
        var runbook = await Runbooks.FirstOrDefaultAsync(x => x.Id == runbookId, cancellationToken);
        if (request is null || runbook is null)
        {
            return null;
        }

        var existing = await RequestRunbooks
            .FirstOrDefaultAsync(x => x.RequestId == requestId && x.RunbookId == runbookId, cancellationToken);
        if (existing is not null)
        {
            return new LinkedRunbookDto(
                existing.Id,
                runbook.Id,
                runbook.Title,
                runbook.SystemName,
                runbook.Category,
                existing.LinkedDate);
        }

        var now = DateTime.UtcNow;
        var link = new RequestRunbook
        {
            RequestId = requestId,
            RunbookId = runbookId,
            LinkedDate = now
        };

        request.UpdatedDate = now;
        RequestRunbooks.Add(link);
        RequestHistory.Add(new RequestHistory
        {
            RequestId = requestId,
            ActionType = "RunbookLinked",
            NewValue = runbook.Title,
            ChangedBy = changedBy,
            ChangedDate = now
        });
        await _db.SaveChangesAsync(cancellationToken);

        return new LinkedRunbookDto(
            link.Id,
            runbook.Id,
            runbook.Title,
            runbook.SystemName,
            runbook.Category,
            link.LinkedDate);
    }

    public async Task<bool> UnlinkRunbookAsync(
        int requestId,
        int runbookId,
        string? changedBy,
        CancellationToken cancellationToken)
    {
        var link = await RequestRunbooks
            .Include(x => x.Runbook)
            .FirstOrDefaultAsync(x => x.RequestId == requestId && x.RunbookId == runbookId, cancellationToken);
        if (link is null)
        {
            return false;
        }

        var request = await Requests.FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);
        var now = DateTime.UtcNow;

        if (request is not null)
        {
            request.UpdatedDate = now;
        }

        RequestRunbooks.Remove(link);
        RequestHistory.Add(new RequestHistory
        {
            RequestId = requestId,
            ActionType = "RunbookUnlinked",
            OldValue = link.Runbook?.Title,
            ChangedBy = changedBy,
            ChangedDate = now
        });
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<RequestDashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken)
    {
        var weekStart = DateTime.UtcNow.Date.AddDays(-6);

        var requestRows = await Requests
            .AsNoTracking()
            .Select(x => new
            {
                x.SystemName,
                x.Priority,
                x.Status,
                x.Type,
                x.CreatedDate
            })
            .ToListAsync(cancellationToken);

        var openP1Count = requestRows.Count(x =>
            x.Priority == RequestPriority.P1 && x.Status != RequestStatus.Done);

        var requestsThisWeek = requestRows.Count(x => x.CreatedDate >= weekStart);

        var bySystem = requestRows
            .Where(x => x.Status != RequestStatus.Done)
            .GroupBy(x => x.SystemName)
            .Select(x => new GroupCountDto(x.Key, x.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        var byType = requestRows
            .Where(x => x.Status != RequestStatus.Done)
            .GroupBy(x => x.Type)
            .Select(x => new GroupCountDto(x.Key.ToString(), x.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        return new RequestDashboardSummaryDto(openP1Count, requestsThisWeek, bySystem, byType);
    }

    public async Task<RequestReportingDashboardDto> GetReportingDashboardAsync(
        RequestReportingFilterDto filters,
        string? currentUserName,
        bool myRequestsOnly,
        CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-6);
        var toDateExclusive = filters.ToDate?.Date.AddDays(1);

        var filteredRequests = Requests
            .AsNoTracking()
            .Include(x => x.RequestNotes)
            .AsQueryable();

        if (filters.FromDate is not null)
        {
            var fromDate = filters.FromDate.Value.Date;
            filteredRequests = filteredRequests.Where(x => x.CreatedDate >= fromDate);
        }

        if (toDateExclusive is not null)
        {
            filteredRequests = filteredRequests.Where(x => x.CreatedDate < toDateExclusive);
        }

        if (!string.IsNullOrWhiteSpace(filters.System))
        {
            filteredRequests = filteredRequests.Where(x => x.SystemName == filters.System);
        }

        if (filters.Priority is not null)
        {
            filteredRequests = filteredRequests.Where(x => x.Priority == filters.Priority);
        }

        if (filters.Status is not null)
        {
            filteredRequests = filteredRequests.Where(x => x.Status == filters.Status);
        }

        if (filters.Type is not null)
        {
            filteredRequests = filteredRequests.Where(x => x.Type == filters.Type);
        }

        if (myRequestsOnly && !string.IsNullOrWhiteSpace(currentUserName))
        {
            filteredRequests = filteredRequests.Where(x => x.OwnerUserName == currentUserName || x.CreatedByUserName == currentUserName || x.SubmittedByUserName == currentUserName);
        }

        var requestRows = await filteredRequests.ToListAsync(cancellationToken);
        var openRequests = requestRows.Where(x => x.Status != RequestStatus.Done).ToList();

        var cards = new RequestReportingCardsDto(
            openRequests.Count(x => x.Priority == RequestPriority.P1),
            openRequests.Count(x => x.Priority == RequestPriority.P2),
            openRequests.Count,
            requestRows.Count(x => x.CreatedDate >= weekStart),
            requestRows.Count(x => x.Status == RequestStatus.Done && x.UpdatedDate >= weekStart),
            openRequests.Count(x => x.Status == RequestStatus.Waiting));

        var openBySystem = openRequests
            .GroupBy(x => x.SystemName)
            .Select(x => new OpenRequestsBySystemDto(
                x.Key,
                x.Count(),
                x.Count(r => r.Priority == RequestPriority.P1),
                x.Count(r => r.Priority == RequestPriority.P2)))
            .OrderByDescending(x => x.P1Count)
            .ThenByDescending(x => x.P2Count)
            .ThenByDescending(x => x.OpenCount)
            .ToList();

        var requestsByType = requestRows
            .GroupBy(x => x.Type)
            .Select(x => new RequestsByTypeDto(x.Key, x.Count()))
            .ToList();

        var existingTypes = requestsByType.Select(x => x.Type).ToHashSet();
        var allTypes = Enum.GetValues<RequestType>()
            .Where(x => !existingTypes.Contains(x))
            .Select(x => new RequestsByTypeDto(x, 0));
        requestsByType = requestsByType
            .Concat(allTypes)
            .OrderBy(x => x.Type)
            .ToList();

        var oldestOpenRequests = openRequests
            .OrderBy(x => x.CreatedDate)
            .Take(10)
            .Select(x => new OldestOpenRequestDto(
                x.Id,
                x.Title,
                x.SystemName,
                x.Priority,
                x.Status,
                x.CreatedDate,
                Math.Max(0, (today - x.CreatedDate.Date).Days)))
            .ToList();

        var waitingRequests = openRequests
            .Where(x => x.Status == RequestStatus.Waiting)
            .OrderBy(x => x.UpdatedDate)
            .Take(20)
            .Select(x => new WaitingRequestDto(
                x.Id,
                x.Title,
                x.SystemName,
                x.RequestNotes
                    .OrderByDescending(n => n.CreatedDate)
                    .Select(n => n.NoteText)
                    .FirstOrDefault(),
                x.UpdatedDate))
            .ToList();

        return new RequestReportingDashboardDto(
            cards,
            openBySystem,
            requestsByType,
            oldestOpenRequests,
            waitingRequests);
    }

    public async Task<WeeklyManagementSummaryDto> GetWeeklyManagementSummaryAsync(string? currentUserName, bool myRequestsOnly, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-6);
        var weekEnd = today;

        var requests = await Requests
            .AsNoTracking()
            .Include(x => x.RequestNotes)
            .ToListAsync(cancellationToken);

        if (myRequestsOnly && !string.IsNullOrWhiteSpace(currentUserName))
        {
            requests = requests
                .Where(x => x.OwnerUserName == currentUserName || x.CreatedByUserName == currentUserName || x.SubmittedByUserName == currentUserName)
                .ToList();
        }

        var systems = await Systems.AsNoTracking().ToListAsync(cancellationToken);

        var completed = requests
            .Where(x => x.Status == RequestStatus.Done && x.UpdatedDate >= weekStart)
            .OrderByDescending(x => x.UpdatedDate)
            .Select(ToWeeklyRequestDto)
            .ToList();

        var created = requests
            .Where(x => x.CreatedDate >= weekStart)
            .OrderByDescending(x => x.CreatedDate)
            .Select(ToWeeklyRequestDto)
            .ToList();

        var openP1P2 = requests
            .Where(x => x.Status != RequestStatus.Done && (x.Priority == RequestPriority.P1 || x.Priority == RequestPriority.P2))
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.CreatedDate)
            .Select(ToWeeklyRequestDto)
            .ToList();

        var waiting = requests
            .Where(x => x.Status == RequestStatus.Waiting)
            .OrderBy(x => x.UpdatedDate)
            .Select(x => new WaitingRequestDto(
                x.Id,
                x.Title,
                x.SystemName,
                x.RequestNotes.OrderByDescending(n => n.CreatedDate).Select(n => n.NoteText).FirstOrDefault(),
                x.UpdatedDate))
            .ToList();

        var systemsCausingMostWork = requests
            .Where(x => x.Status != RequestStatus.Done)
            .GroupBy(x => x.SystemName)
            .Select(x => new OpenRequestsBySystemDto(
                x.Key,
                x.Count(),
                x.Count(r => r.Priority == RequestPriority.P1),
                x.Count(r => r.Priority == RequestPriority.P2)))
            .OrderByDescending(x => x.OpenCount)
            .Take(5)
            .ToList();

        var keyRisks = systems
            .Where(x => x.Criticality == SystemCriticality.Critical || x.Criticality == SystemCriticality.High || !string.IsNullOrWhiteSpace(x.KnownRisks))
            .OrderByDescending(x => x.Criticality)
            .ThenBy(x => x.Name)
            .Take(8)
            .Select(ToSystemRiskSummaryDto)
            .ToList();

        var talkingPoints = new List<string>
        {
            $"Completed {completed.Count} request(s) this week.",
            $"Received {created.Count} new request(s) this week.",
            $"{openP1P2.Count} open P1/P2 request(s) need visibility.",
            $"{waiting.Count} request(s) are waiting or blocked."
        };

        if (systemsCausingMostWork.FirstOrDefault() is { } busiestSystem)
        {
            talkingPoints.Add($"{busiestSystem.SystemName} is currently generating the most open support demand.");
        }

        if (keyRisks.Count > 0)
        {
            talkingPoints.Add($"{keyRisks.Count} system risk item(s) should remain visible to management.");
        }

        return new WeeklyManagementSummaryDto(
            weekStart,
            weekEnd,
            completed,
            created,
            openP1P2,
            waiting,
            systemsCausingMostWork,
            keyRisks,
            talkingPoints);
    }

    public async Task<SystemRiskDashboardDto> GetSystemRiskDashboardAsync(CancellationToken cancellationToken)
    {
        var systems = await Systems.AsNoTracking().ToListAsync(cancellationToken);
        var requests = await Requests.AsNoTracking().ToListAsync(cancellationToken);
        var recurringIssues = await RecurringIssues.AsNoTracking().ToListAsync(cancellationToken);

        var highRiskSystems = systems
            .Where(x => x.Criticality == SystemCriticality.Critical || x.Criticality == SystemCriticality.High)
            .OrderByDescending(x => x.Criticality)
            .ThenBy(x => x.Name)
            .Select(ToSystemRiskSummaryDto)
            .ToList();

        var systemsWithMostOpenRequests = requests
            .Where(x => x.Status != RequestStatus.Done)
            .GroupBy(x => x.SystemName)
            .Select(x => new SystemWorkloadRiskDto(
                x.Key,
                x.Count(),
                x.Count(r => r.Priority == RequestPriority.P1),
                x.Count(r => r.Priority == RequestPriority.P2)))
            .OrderByDescending(x => x.OpenCount)
            .Take(10)
            .ToList();

        var systemsWithMostP1P2Requests = systemsWithMostOpenRequests
            .Where(x => x.P1Count > 0 || x.P2Count > 0)
            .OrderByDescending(x => x.P1Count)
            .ThenByDescending(x => x.P2Count)
            .Take(10)
            .ToList();

        var systemsWithRepeatedIssues = recurringIssues
            .GroupBy(x => x.SystemName)
            .Select(x => new SystemRecurringIssueRiskDto(
                x.Key,
                x.Count(),
                x.Sum(i => i.RecurrenceCount),
                x.Any(i => i.PermanentFixNeeded)))
            .OrderByDescending(x => x.PermanentFixNeeded)
            .ThenByDescending(x => x.TotalRecurrences)
            .Take(10)
            .ToList();

        var systemsWithKnownRisks = systems
            .Where(x => !string.IsNullOrWhiteSpace(x.KnownRisks))
            .OrderByDescending(x => x.Criticality)
            .ThenBy(x => x.Name)
            .Select(ToSystemRiskSummaryDto)
            .ToList();

        return new SystemRiskDashboardDto(
            highRiskSystems,
            systemsWithMostOpenRequests,
            systemsWithMostP1P2Requests,
            systemsWithRepeatedIssues,
            systemsWithKnownRisks);
    }

    private static WeeklyRequestDto ToWeeklyRequestDto(EngineeringRequest request)
    {
        return new WeeklyRequestDto(
            request.Id,
            request.Title,
            request.SystemName,
            request.Priority,
            request.Status,
            request.Type,
            request.CreatedDate,
            request.UpdatedDate);
    }

    private static SystemRiskSummaryDto ToSystemRiskSummaryDto(EngineeringSystem system)
    {
        return new SystemRiskSummaryDto(
            system.Id,
            system.Name,
            system.Purpose,
            system.MainUsers,
            system.Criticality,
            system.KnownRisks,
            system.Notes);
    }
}
