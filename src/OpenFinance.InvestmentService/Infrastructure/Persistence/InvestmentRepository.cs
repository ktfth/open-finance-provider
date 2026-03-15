using Microsoft.EntityFrameworkCore;
using OpenFinance.InvestmentService.Domain.Entities;
using OpenFinance.InvestmentService.Domain.Repositories;

namespace OpenFinance.InvestmentService.Infrastructure.Persistence;

public class InvestmentRepository(InvestmentDbContext context) : IInvestmentRepository
{
    // ── Fixed Income ──────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<FixedIncomeInvestment>> GetFixedIncomeByUserIdAsync(
        string userId, CancellationToken ct = default) =>
        await context.FixedIncomeInvestments
            .Where(i => i.UserId == userId)
            .ToListAsync(ct);

    public async Task<FixedIncomeInvestment?> GetFixedIncomeByIdAsync(
        Guid investmentId, CancellationToken ct = default) =>
        await context.FixedIncomeInvestments
            .FirstOrDefaultAsync(i => i.Id == investmentId, ct);

    public async Task<FixedIncomeBalance?> GetFixedIncomeBalanceAsync(
        Guid investmentId, CancellationToken ct = default) =>
        await context.FixedIncomeBalances
            .FirstOrDefaultAsync(b => b.InvestmentId == investmentId, ct);

    public async Task<IReadOnlyList<FixedIncomeTransaction>> GetFixedIncomeTransactionsAsync(
        Guid investmentId, DateOnly from, DateOnly to, CancellationToken ct = default) =>
        await context.FixedIncomeTransactions
            .Where(t => t.InvestmentId == investmentId
                     && t.TransactionDate >= from
                     && t.TransactionDate <= to)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(ct);

    // ── Variable Income ───────────────────────────────────────────────────────

    public async Task<IReadOnlyList<VariableIncomeInvestment>> GetVariableIncomeByUserIdAsync(
        string userId, CancellationToken ct = default) =>
        await context.VariableIncomeInvestments
            .Where(i => i.UserId == userId)
            .ToListAsync(ct);

    public async Task<VariableIncomeInvestment?> GetVariableIncomeByIdAsync(
        Guid investmentId, CancellationToken ct = default) =>
        await context.VariableIncomeInvestments
            .FirstOrDefaultAsync(i => i.Id == investmentId, ct);

    public async Task<IReadOnlyList<VariableIncomeTransaction>> GetVariableIncomeTransactionsAsync(
        Guid investmentId, DateOnly from, DateOnly to, CancellationToken ct = default) =>
        await context.VariableIncomeTransactions
            .Where(t => t.InvestmentId == investmentId
                     && t.TransactionDate >= from
                     && t.TransactionDate <= to)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(ct);

    // ── Treasury Bonds ────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<TreasuryBondInvestment>> GetTreasuryBondsByUserIdAsync(
        string userId, CancellationToken ct = default) =>
        await context.TreasuryBondInvestments
            .Where(i => i.UserId == userId)
            .ToListAsync(ct);

    public async Task<TreasuryBondInvestment?> GetTreasuryBondByIdAsync(
        Guid investmentId, CancellationToken ct = default) =>
        await context.TreasuryBondInvestments
            .FirstOrDefaultAsync(i => i.Id == investmentId, ct);
}
