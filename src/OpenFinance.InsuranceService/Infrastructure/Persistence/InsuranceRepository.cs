using Microsoft.EntityFrameworkCore;
using OpenFinance.InsuranceService.Domain.Entities;
using OpenFinance.InsuranceService.Domain.Repositories;

namespace OpenFinance.InsuranceService.Infrastructure.Persistence;

public class InsuranceRepository(InsuranceDbContext context) : IInsuranceRepository
{
    public async Task<IReadOnlyList<Insurance>> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        await context.Insurances
            .Where(i => i.UserId == userId)
            .ToListAsync(ct);

    public async Task<Insurance?> GetByIdAsync(Guid insuranceId, CancellationToken ct = default) =>
        await context.Insurances.FirstOrDefaultAsync(i => i.Id == insuranceId, ct);

    public async Task<IReadOnlyList<InsuranceClaim>> GetClaimsAsync(Guid insuranceId, CancellationToken ct = default) =>
        await context.InsuranceClaims
            .Where(c => c.InsuranceId == insuranceId)
            .OrderByDescending(c => c.NotificationDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<InsuranceCoverage>> GetCoveragesAsync(Guid insuranceId, CancellationToken ct = default) =>
        await context.InsuranceCoverages
            .Where(c => c.InsuranceId == insuranceId)
            .OrderByDescending(c => c.IsMainCoverage)
            .ThenBy(c => c.CoverageName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<InsurancePremiumPayment>> GetPremiumPaymentsAsync(Guid insuranceId, CancellationToken ct = default) =>
        await context.InsurancePremiumPayments
            .Where(p => p.InsuranceId == insuranceId)
            .OrderBy(p => p.DueDate)
            .ToListAsync(ct);

    public async Task AddAsync(Insurance insurance, CancellationToken ct = default)
    {
        await context.Insurances.AddAsync(insurance, ct);
        await context.SaveChangesAsync(ct);
    }
}
