using Microsoft.EntityFrameworkCore;
using OpenFinance.LoanService.Domain.Entities;
using OpenFinance.LoanService.Domain.Repositories;

namespace OpenFinance.LoanService.Infrastructure.Persistence;

public class LoanRepository(LoanDbContext context) : ILoanRepository
{
    public async Task<IReadOnlyList<LoanContract>> GetLoansByUserIdAsync(string userId, CancellationToken ct = default) =>
        await context.LoanContracts.Where(l => l.UserId == userId).ToListAsync(ct);

    public async Task<LoanContract?> GetLoanByIdAsync(Guid contractId, CancellationToken ct = default) =>
        await context.LoanContracts.FirstOrDefaultAsync(l => l.Id == contractId, ct);

    public async Task<IReadOnlyList<LoanPayment>> GetLoanPaymentsAsync(Guid contractId, CancellationToken ct = default) =>
        await context.LoanPayments.Where(p => p.ContractId == contractId).ToListAsync(ct);

    public async Task<IReadOnlyList<LoanInstalment>> GetLoanInstalmentsAsync(Guid contractId, CancellationToken ct = default) =>
        await context.LoanInstalments.Where(i => i.ContractId == contractId).OrderBy(i => i.InstalmentNumber).ToListAsync(ct);

    public async Task<IReadOnlyList<LoanWarranty>> GetLoanWarrantiesAsync(Guid contractId, CancellationToken ct = default) =>
        await context.LoanWarranties.Where(w => w.ContractId == contractId).ToListAsync(ct);

    public async Task<IReadOnlyList<FinancingContract>> GetFinancingsByUserIdAsync(string userId, CancellationToken ct = default) =>
        await context.FinancingContracts.Where(f => f.UserId == userId).ToListAsync(ct);

    public async Task<FinancingContract?> GetFinancingByIdAsync(Guid contractId, CancellationToken ct = default) =>
        await context.FinancingContracts.FirstOrDefaultAsync(f => f.Id == contractId, ct);

    public async Task<IReadOnlyList<OverdraftContract>> GetOverdraftsByUserIdAsync(string userId, CancellationToken ct = default) =>
        await context.OverdraftContracts.Where(o => o.UserId == userId).ToListAsync(ct);

    public async Task<OverdraftContract?> GetOverdraftByIdAsync(Guid contractId, CancellationToken ct = default) =>
        await context.OverdraftContracts.FirstOrDefaultAsync(o => o.Id == contractId, ct);

    public async Task AddLoanAsync(LoanContract loan, CancellationToken ct = default)
    {
        await context.LoanContracts.AddAsync(loan, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task AddFinancingAsync(FinancingContract financing, CancellationToken ct = default)
    {
        await context.FinancingContracts.AddAsync(financing, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task AddOverdraftAsync(OverdraftContract overdraft, CancellationToken ct = default)
    {
        await context.OverdraftContracts.AddAsync(overdraft, ct);
        await context.SaveChangesAsync(ct);
    }
}
