using OpenFinance.CapitalizationService.Domain.Entities;

namespace OpenFinance.CapitalizationService.Domain.Repositories;

public interface ICapitalizationRepository
{
    Task<IReadOnlyList<CapitalizationBond>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<CapitalizationBond?> GetByIdAsync(Guid bondId, CancellationToken ct = default);
    Task<IReadOnlyList<CapitalizationPayment>> GetPaymentsAsync(Guid bondId, CancellationToken ct = default);
    Task AddAsync(CapitalizationBond bond, CancellationToken ct = default);
}
