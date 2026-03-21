using OpenFinance.ResourcesService.Domain.Entities;

namespace OpenFinance.ResourcesService.Domain.Repositories;

public interface IResourceRepository
{
    Task<IReadOnlyList<Resource>> GetByConsentIdAsync(Guid consentId, CancellationToken ct = default);
    Task AddAsync(Resource resource, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Resource> resources, CancellationToken ct = default);
}
