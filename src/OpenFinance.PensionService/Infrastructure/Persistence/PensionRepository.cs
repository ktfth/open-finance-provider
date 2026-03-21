using Microsoft.EntityFrameworkCore;
using OpenFinance.PensionService.Domain.Entities;
using OpenFinance.PensionService.Domain.Repositories;

namespace OpenFinance.PensionService.Infrastructure.Persistence;

public class PensionRepository(PensionDbContext context) : IPensionRepository
{
    public async Task<IReadOnlyList<Pension>> GetByUserIdAsync(
        string userId, CancellationToken ct = default) =>
        await context.Pensions
            .Where(p => p.UserId == userId)
            .ToListAsync(ct);

    public async Task<Pension?> GetByIdAsync(
        Guid pensionId, CancellationToken ct = default) =>
        await context.Pensions
            .FirstOrDefaultAsync(p => p.Id == pensionId, ct);

    public async Task<PensionBalance?> GetBalanceAsync(
        Guid pensionId, CancellationToken ct = default) =>
        await context.PensionBalances
            .FirstOrDefaultAsync(b => b.PensionId == pensionId, ct);

    public async Task<IReadOnlyList<PensionContribution>> GetContributionsAsync(
        Guid pensionId, CancellationToken ct = default) =>
        await context.PensionContributions
            .Where(c => c.PensionId == pensionId)
            .OrderByDescending(c => c.ContributionDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<PensionWithdrawal>> GetWithdrawalsAsync(
        Guid pensionId, CancellationToken ct = default) =>
        await context.PensionWithdrawals
            .Where(w => w.PensionId == pensionId)
            .OrderByDescending(w => w.WithdrawalDate)
            .ToListAsync(ct);

    public async Task AddAsync(Pension pension, CancellationToken ct = default)
    {
        await context.Pensions.AddAsync(pension, ct);
        await context.SaveChangesAsync(ct);
    }
}
