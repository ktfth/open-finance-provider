using Microsoft.EntityFrameworkCore;
using OpenFinance.ResourcesService.Domain.Entities;
using OpenFinance.ResourcesService.Domain.Repositories;

namespace OpenFinance.ResourcesService.Infrastructure.Persistence;

public class ResourceRepository(ResourceDbContext context) : IResourceRepository
{
    public async Task<IReadOnlyList<Resource>> GetByConsentIdAsync(Guid consentId, CancellationToken ct = default) =>
        await context.Resources.Where(r => r.ConsentId == consentId).ToListAsync(ct);

    public async Task AddAsync(Resource resource, CancellationToken ct = default)
    {
        await context.Resources.AddAsync(resource, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<Resource> resources, CancellationToken ct = default)
    {
        await context.Resources.AddRangeAsync(resources, ct);
        await context.SaveChangesAsync(ct);
    }
}
