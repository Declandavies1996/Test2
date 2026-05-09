using CaeDashboard.EngineeringRequests.Dtos;
using CaeDashboard.EngineeringRequests.Models;
using Microsoft.EntityFrameworkCore;

namespace CaeDashboard.EngineeringRequests.Services;

public class RunbookService<TDbContext> where TDbContext : DbContext
{
    private readonly TDbContext _db;

    public RunbookService(TDbContext db)
    {
        _db = db;
    }

    private DbSet<Runbook> Runbooks => _db.Set<Runbook>();

    public async Task<IReadOnlyList<RunbookListItemDto>> GetRunbooksAsync(
        string? search,
        string? system,
        RunbookCategory? category,
        CancellationToken cancellationToken)
    {
        var query = Runbooks.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.Title.Contains(term) ||
                x.SystemName.Contains(term) ||
                (x.Symptoms != null && x.Symptoms.Contains(term)) ||
                (x.Cause != null && x.Cause.Contains(term)) ||
                (x.ResolutionSteps != null && x.ResolutionSteps.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(system))
        {
            query = query.Where(x => x.SystemName == system);
        }

        if (category is not null)
        {
            query = query.Where(x => x.Category == category);
        }

        return await query
            .OrderBy(x => x.SystemName)
            .ThenBy(x => x.Category)
            .ThenBy(x => x.Title)
            .Select(x => new RunbookListItemDto(
                x.Id,
                x.Title,
                x.SystemName,
                x.Category,
                x.UpdatedDate))
            .ToListAsync(cancellationToken);
    }

    public async Task<RunbookDetailDto?> GetRunbookAsync(int id, CancellationToken cancellationToken)
    {
        return await Runbooks
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new RunbookDetailDto(
                x.Id,
                x.Title,
                x.SystemName,
                x.Category,
                x.Problem,
                x.Symptoms,
                x.Cause,
                x.FixSteps,
                x.ResolutionSteps,
                x.VerificationSteps,
                x.KnownRisks,
                x.Notes,
                x.CreatedDate,
                x.UpdatedDate,
                x.LastUpdated))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CreateRunbookAsync(UpsertRunbookDto dto, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var runbook = new Runbook
        {
            Title = dto.Title.Trim(),
            SystemName = dto.SystemName.Trim(),
            Category = dto.Category,
            Problem = dto.Problem,
            Symptoms = dto.Symptoms,
            Cause = dto.Cause,
            FixSteps = dto.FixSteps,
            ResolutionSteps = dto.ResolutionSteps,
            VerificationSteps = dto.VerificationSteps,
            KnownRisks = dto.KnownRisks,
            Notes = dto.Notes,
            CreatedDate = now,
            UpdatedDate = now,
            LastUpdated = now
        };

        Runbooks.Add(runbook);
        await _db.SaveChangesAsync(cancellationToken);
        return runbook.Id;
    }

    public async Task<bool> UpdateRunbookAsync(int id, UpsertRunbookDto dto, CancellationToken cancellationToken)
    {
        var runbook = await Runbooks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (runbook is null)
        {
            return false;
        }

        runbook.Title = dto.Title.Trim();
        runbook.SystemName = dto.SystemName.Trim();
        runbook.Category = dto.Category;
        runbook.Problem = dto.Problem;
        runbook.Symptoms = dto.Symptoms;
        runbook.Cause = dto.Cause;
        runbook.FixSteps = dto.FixSteps;
        runbook.ResolutionSteps = dto.ResolutionSteps;
        runbook.VerificationSteps = dto.VerificationSteps;
        runbook.KnownRisks = dto.KnownRisks;
        runbook.Notes = dto.Notes;
        runbook.UpdatedDate = DateTime.UtcNow;
        runbook.LastUpdated = runbook.UpdatedDate;

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteRunbookAsync(int id, CancellationToken cancellationToken)
    {
        var runbook = await Runbooks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (runbook is null)
        {
            return false;
        }

        Runbooks.Remove(runbook);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
