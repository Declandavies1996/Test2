using CaeDashboard.EngineeringRequests.Dtos;
using CaeDashboard.EngineeringRequests.Models;
using Microsoft.EntityFrameworkCore;

namespace CaeDashboard.EngineeringRequests.Services;

public class EngineeringRequestService<TDbContext> where TDbContext : DbContext
{
    private readonly TDbContext _db;

    public EngineeringRequestService(TDbContext db)
    {
        _db = db;
    }

    private DbSet<EngineeringRequest> Requests => _db.Set<EngineeringRequest>();
    private DbSet<RequestNote> RequestNotes => _db.Set<RequestNote>();

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
        return request.Id;
    }

    public async Task<bool> UpdateRequestAsync(int id, UpsertEngineeringRequestDto dto, CancellationToken cancellationToken)
    {
        var request = await Requests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (request is null)
        {
            return false;
        }

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
        await _db.SaveChangesAsync(cancellationToken);

        return new RequestNoteDto(note.Id, note.RequestId, note.NoteText, note.CreatedBy, note.CreatedDate);
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
