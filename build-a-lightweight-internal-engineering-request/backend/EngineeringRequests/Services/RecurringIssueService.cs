using CaeDashboard.EngineeringRequests.Dtos;
using CaeDashboard.EngineeringRequests.Models;
using Microsoft.EntityFrameworkCore;

namespace CaeDashboard.EngineeringRequests.Services;

public class RecurringIssueService<TDbContext> where TDbContext : DbContext
{
    private readonly TDbContext _db;

    public RecurringIssueService(TDbContext db)
    {
        _db = db;
    }

    private DbSet<RecurringIssue> RecurringIssues => _db.Set<RecurringIssue>();

    public async Task<IReadOnlyList<RecurringIssueDto>> GetIssuesAsync(string? search, string? system, CancellationToken cancellationToken)
    {
        var query = RecurringIssues.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.IssueSummary.Contains(term) ||
                (x.TemporaryFix != null && x.TemporaryFix.Contains(term)) ||
                (x.SuspectedRootCause != null && x.SuspectedRootCause.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(system))
        {
            query = query.Where(x => x.SystemName == system);
        }

        return await query
            .OrderByDescending(x => x.PermanentFixNeeded)
            .ThenByDescending(x => x.RecurrenceCount)
            .ThenBy(x => x.SystemName)
            .Select(x => new RecurringIssueDto(
                x.Id, x.SystemName, x.IssueSummary, x.RecurrenceCount, x.TemporaryFix,
                x.SuspectedRootCause, x.PermanentFixNeeded, x.RelatedRequestIds,
                x.CreatedDate, x.UpdatedDate))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CreateIssueAsync(UpsertRecurringIssueDto dto, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var issue = new RecurringIssue
        {
            SystemName = dto.SystemName.Trim(),
            IssueSummary = dto.IssueSummary.Trim(),
            RecurrenceCount = Math.Max(1, dto.RecurrenceCount),
            TemporaryFix = dto.TemporaryFix,
            SuspectedRootCause = dto.SuspectedRootCause,
            PermanentFixNeeded = dto.PermanentFixNeeded,
            RelatedRequestIds = dto.RelatedRequestIds,
            CreatedDate = now,
            UpdatedDate = now
        };

        RecurringIssues.Add(issue);
        await _db.SaveChangesAsync(cancellationToken);
        return issue.Id;
    }

    public async Task<bool> UpdateIssueAsync(int id, UpsertRecurringIssueDto dto, CancellationToken cancellationToken)
    {
        var issue = await RecurringIssues.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (issue is null)
        {
            return false;
        }

        issue.SystemName = dto.SystemName.Trim();
        issue.IssueSummary = dto.IssueSummary.Trim();
        issue.RecurrenceCount = Math.Max(1, dto.RecurrenceCount);
        issue.TemporaryFix = dto.TemporaryFix;
        issue.SuspectedRootCause = dto.SuspectedRootCause;
        issue.PermanentFixNeeded = dto.PermanentFixNeeded;
        issue.RelatedRequestIds = dto.RelatedRequestIds;
        issue.UpdatedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteIssueAsync(int id, CancellationToken cancellationToken)
    {
        var issue = await RecurringIssues.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (issue is null)
        {
            return false;
        }

        RecurringIssues.Remove(issue);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
