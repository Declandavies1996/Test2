using System.Security.Cryptography;
using CaeDashboard.EngineeringRequests.Dtos;
using CaeDashboard.EngineeringRequests.Models;
using Microsoft.EntityFrameworkCore;

namespace CaeDashboard.EngineeringRequests.Services;

public class SubmissionLinkService<TDbContext> where TDbContext : DbContext
{
    private readonly TDbContext _db;

    public SubmissionLinkService(TDbContext db)
    {
        _db = db;
    }

    private DbSet<SubmissionLink> SubmissionLinks => _db.Set<SubmissionLink>();

    public async Task<SubmissionLinkDto> GetOrCreateMyLinkAsync(string currentUserName, CancellationToken cancellationToken)
    {
        var existing = await SubmissionLinks
            .AsNoTracking()
            .Where(x => x.OwnerUserName == currentUserName && x.IsActive)
            .OrderByDescending(x => x.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            return ToDto(existing);
        }

        var link = new SubmissionLink
        {
            OwnerUserName = currentUserName,
            Token = await CreateUniqueTokenAsync(cancellationToken),
            DisplayName = currentUserName,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedByUserName = currentUserName
        };

        SubmissionLinks.Add(link);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(link);
    }

    public async Task<SubmissionLinkPublicDto?> GetPublicLinkAsync(string token, CancellationToken cancellationToken)
    {
        return await SubmissionLinks
            .AsNoTracking()
            .Where(x => x.Token == token && x.IsActive)
            .Select(x => new SubmissionLinkPublicDto(x.Token, x.DisplayName))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<string?> GetOwnerForTokenAsync(string? token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        return await SubmissionLinks
            .AsNoTracking()
            .Where(x => x.Token == token && x.IsActive)
            .Select(x => x.OwnerUserName)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<string> CreateUniqueTokenAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
            var exists = await SubmissionLinks.AnyAsync(x => x.Token == token, cancellationToken);
            if (!exists)
            {
                return token;
            }
        }
    }

    private static SubmissionLinkDto ToDto(SubmissionLink link)
    {
        return new SubmissionLinkDto(
            link.Id,
            link.OwnerUserName,
            link.Token,
            link.DisplayName,
            link.IsActive,
            link.CreatedDate,
            link.CreatedByUserName);
    }
}
