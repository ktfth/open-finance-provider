using Microsoft.EntityFrameworkCore;
using OpenFinance.CapitalizationService.Domain.Entities;
using OpenFinance.CapitalizationService.Domain.Repositories;

namespace OpenFinance.CapitalizationService.Infrastructure.Persistence;

public class CapitalizationRepository(CapitalizationDbContext context) : ICapitalizationRepository
{
    public async Task<IReadOnlyList<CapitalizationBond>> GetByUserIdAsync(
        string userId, CancellationToken ct = default) =>
        await context.CapitalizationBonds
            .Where(b => b.UserId == userId)
            .ToListAsync(ct);

    public async Task<CapitalizationBond?> GetByIdAsync(
        Guid bondId, CancellationToken ct = default) =>
        await context.CapitalizationBonds
            .FirstOrDefaultAsync(b => b.Id == bondId, ct);

    public async Task<IReadOnlyList<CapitalizationPayment>> GetPaymentsAsync(
        Guid bondId, CancellationToken ct = default) =>
        await context.CapitalizationPayments
            .Where(p => p.BondId == bondId)
            .OrderBy(p => p.DueDate)
            .ToListAsync(ct);

    public async Task AddAsync(CapitalizationBond bond, CancellationToken ct = default)
    {
        await context.CapitalizationBonds.AddAsync(bond, ct);
        await context.SaveChangesAsync(ct);
    }
}
