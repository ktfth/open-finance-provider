using OpenFinance.ConsentService.Domain.Entities;

namespace OpenFinance.ConsentService.Domain.Repositories;

public interface IConsentRepository
{
    Task<Consent?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Consent>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task AddAsync(Consent consent, CancellationToken ct = default);
    Task UpdateAsync(Consent consent, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
