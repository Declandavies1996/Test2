using CaeDashboard.EngineeringRequests.Dtos;
using CaeDashboard.EngineeringRequests.Models;
using Microsoft.EntityFrameworkCore;

namespace CaeDashboard.EngineeringRequests.Services;

public class EngineeringSystemService<TDbContext> where TDbContext : DbContext
{
    private readonly TDbContext _db;

    public EngineeringSystemService(TDbContext db)
    {
        _db = db;
    }

    private DbSet<EngineeringSystem> Systems => _db.Set<EngineeringSystem>();

    public async Task<IReadOnlyList<EngineeringSystemDto>> GetSystemsAsync(CancellationToken cancellationToken)
    {
        return await Systems
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new EngineeringSystemDto(
                x.Id,
                x.Name,
                x.Purpose,
                x.MainUsers,
                x.Criticality,
                x.KnownRisks,
                x.Notes))
            .ToListAsync(cancellationToken);
    }

    public async Task<EngineeringSystemDto?> GetSystemAsync(int id, CancellationToken cancellationToken)
    {
        return await Systems
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new EngineeringSystemDto(
                x.Id,
                x.Name,
                x.Purpose,
                x.MainUsers,
                x.Criticality,
                x.KnownRisks,
                x.Notes))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CreateSystemAsync(UpsertEngineeringSystemDto dto, CancellationToken cancellationToken)
    {
        var system = new EngineeringSystem
        {
            Name = dto.Name.Trim(),
            Purpose = dto.Purpose,
            MainUsers = dto.MainUsers,
            Criticality = dto.Criticality,
            KnownRisks = dto.KnownRisks,
            Notes = dto.Notes
        };

        Systems.Add(system);
        await _db.SaveChangesAsync(cancellationToken);
        return system.Id;
    }

    public async Task<bool> UpdateSystemAsync(int id, UpsertEngineeringSystemDto dto, CancellationToken cancellationToken)
    {
        var system = await Systems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (system is null)
        {
            return false;
        }

        system.Name = dto.Name.Trim();
        system.Purpose = dto.Purpose;
        system.MainUsers = dto.MainUsers;
        system.Criticality = dto.Criticality;
        system.KnownRisks = dto.KnownRisks;
        system.Notes = dto.Notes;

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteSystemAsync(int id, CancellationToken cancellationToken)
    {
        var system = await Systems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (system is null)
        {
            return false;
        }

        Systems.Remove(system);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
