using OpenFinance.PensionService.Domain.Entities;

namespace OpenFinance.PensionService.Domain.Repositories;

/// <summary>
/// Abstraction for pension plan data access operations.
/// </summary>
public interface IPensionRepository
{
    Task<IReadOnlyList<Pension>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<Pension?> GetByIdAsync(Guid pensionId, CancellationToken ct = default);
    Task<PensionBalance?> GetBalanceAsync(Guid pensionId, CancellationToken ct = default);
    Task<IReadOnlyList<PensionContribution>> GetContributionsAsync(Guid pensionId, CancellationToken ct = default);
    Task<IReadOnlyList<PensionWithdrawal>> GetWithdrawalsAsync(Guid pensionId, CancellationToken ct = default);
    Task AddAsync(Pension pension, CancellationToken ct = default);
}
