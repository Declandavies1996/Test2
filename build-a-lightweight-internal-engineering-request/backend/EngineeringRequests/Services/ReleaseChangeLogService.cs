using CaeDashboard.EngineeringRequests.Dtos;
using CaeDashboard.EngineeringRequests.Models;
using Microsoft.EntityFrameworkCore;

namespace CaeDashboard.EngineeringRequests.Services;

public class ReleaseChangeLogService<TDbContext> where TDbContext : DbContext
{
    private readonly TDbContext _db;

    public ReleaseChangeLogService(TDbContext db)
    {
        _db = db;
    }

    private DbSet<ReleaseChangeLog> ReleaseChangeLogs => _db.Set<ReleaseChangeLog>();

    public async Task<IReadOnlyList<ReleaseChangeLogDto>> GetLogsAsync(string? system, int? requestId, CancellationToken cancellationToken)
    {
        var query = ReleaseChangeLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(system))
        {
            query = query.Where(x => x.SystemName == system);
        }

        if (requestId is not null)
        {
            query = query.Where(x => x.RequestId == requestId);
        }

        return await query
            .OrderByDescending(x => x.ReleaseDate)
            .Select(x => new ReleaseChangeLogDto(
                x.Id, x.RequestId, x.SystemName, x.ReleaseDate, x.Summary,
                x.FilesChanged, x.DeploymentNotes, x.RollbackNotes, x.VerifiedBy))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CreateLogAsync(UpsertReleaseChangeLogDto dto, CancellationToken cancellationToken)
    {
        var log = new ReleaseChangeLog
        {
            RequestId = dto.RequestId,
            SystemName = dto.SystemName.Trim(),
            ReleaseDate = dto.ReleaseDate,
            Summary = dto.Summary.Trim(),
            FilesChanged = dto.FilesChanged,
            DeploymentNotes = dto.DeploymentNotes,
            RollbackNotes = dto.RollbackNotes,
            VerifiedBy = dto.VerifiedBy
        };

        ReleaseChangeLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);
        return log.Id;
    }

    public async Task<bool> UpdateLogAsync(int id, UpsertReleaseChangeLogDto dto, CancellationToken cancellationToken)
    {
        var log = await ReleaseChangeLogs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (log is null)
        {
            return false;
        }

        log.RequestId = dto.RequestId;
        log.SystemName = dto.SystemName.Trim();
        log.ReleaseDate = dto.ReleaseDate;
        log.Summary = dto.Summary.Trim();
        log.FilesChanged = dto.FilesChanged;
        log.DeploymentNotes = dto.DeploymentNotes;
        log.RollbackNotes = dto.RollbackNotes;
        log.VerifiedBy = dto.VerifiedBy;

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteLogAsync(int id, CancellationToken cancellationToken)
    {
        var log = await ReleaseChangeLogs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (log is null)
        {
            return false;
        }

        ReleaseChangeLogs.Remove(log);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
