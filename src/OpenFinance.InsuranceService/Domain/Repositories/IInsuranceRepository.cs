using OpenFinance.InsuranceService.Domain.Entities;

namespace OpenFinance.InsuranceService.Domain.Repositories;

/// <summary>
/// Repository abstraction for insurance data access.
/// </summary>
public interface IInsuranceRepository
{
    Task<IReadOnlyList<Insurance>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<Insurance?> GetByIdAsync(Guid insuranceId, CancellationToken ct = default);
    Task<IReadOnlyList<InsuranceClaim>> GetClaimsAsync(Guid insuranceId, CancellationToken ct = default);
    Task<IReadOnlyList<InsuranceCoverage>> GetCoveragesAsync(Guid insuranceId, CancellationToken ct = default);
    Task<IReadOnlyList<InsurancePremiumPayment>> GetPremiumPaymentsAsync(Guid insuranceId, CancellationToken ct = default);
    Task AddAsync(Insurance insurance, CancellationToken ct = default);
}
