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

    public async Task<IReadOnlyList<EngineeringRequestListItemDto>> GetRequestsAsync(
        string? search,
        RequestStatus? status,
        string? system,
        CancellationToken cancellationToken)
    {
        var query = Requests.AsNoTracking();

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
                x.UpdatedDate))
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

    public async Task<int> CreateRequestAsync(UpsertEngineeringRequestDto dto, CancellationToken cancellationToken)
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
            ChangedBy = dto.RequestedBy,
            ChangedDate = now
        });
        await _db.SaveChangesAsync(cancellationToken);

        return request.Id;
    }

    public async Task<bool> UpdateRequestAsync(int id, UpsertEngineeringRequestDto dto, CancellationToken cancellationToken)
    {
        var request = await Requests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (request is null)
        {
            return false;
        }

        var oldStatus = request.Status;
        var oldPriority = request.Priority;

        request.Title = dto.Title.Trim();
        request.Description = dto.Description;
        request.SystemName = dto.SystemName.Trim();
        request.RequestedBy = dto.RequestedBy;
        request.Department = dto.Department;
        request.Priority = dto.Priority;
        request.Status = dto.Status;
        request.Type = dto.Type;
        request.Notes = dto.Notes;
        request.UpdatedDate = DateTime.UtcNow;

        if (oldStatus != request.Status)
        {
            RequestHistory.Add(new RequestHistory
            {
                RequestId = request.Id,
                ActionType = "StatusChanged",
                OldValue = oldStatus.ToString(),
                NewValue = request.Status.ToString(),
                ChangedBy = dto.RequestedBy,
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
                ChangedBy = dto.RequestedBy,
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

    public async Task<RequestNoteDto?> AddNoteAsync(int requestId, AddRequestNoteDto dto, CancellationToken cancellationToken)
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
            CreatedBy = dto.CreatedBy,
            CreatedDate = DateTime.UtcNow
        };

        request.UpdatedDate = DateTime.UtcNow;
        RequestNotes.Add(note);
        RequestHistory.Add(new RequestHistory
        {
            RequestId = requestId,
            ActionType = "NoteAdded",
            NewValue = note.NoteText.Length > 250 ? note.NoteText[..250] : note.NoteText,
            ChangedBy = dto.CreatedBy,
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

        var openP1Count = await Requests.CountAsync(
            x => x.Priority == RequestPriority.P1 && x.Status != RequestStatus.Done,
            cancellationToken);

        var requestsThisWeek = await Requests.CountAsync(x => x.CreatedDate >= weekStart, cancellationToken);

        var bySystem = await Requests
            .AsNoTracking()
            .Where(x => x.Status != RequestStatus.Done)
            .GroupBy(x => x.SystemName)
            .Select(x => new GroupCountDto(x.Key, x.Count()))
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var byType = await Requests
            .AsNoTracking()
            .Where(x => x.Status != RequestStatus.Done)
            .GroupBy(x => x.Type)
            .Select(x => new GroupCountDto(x.Key.ToString(), x.Count()))
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return new RequestDashboardSummaryDto(openP1Count, requestsThisWeek, bySystem, byType);
    }
}
