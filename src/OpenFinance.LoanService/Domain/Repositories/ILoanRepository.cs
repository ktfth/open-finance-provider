using OpenFinance.LoanService.Domain.Entities;

namespace OpenFinance.LoanService.Domain.Repositories;

public interface ILoanRepository
{
    Task<IReadOnlyList<LoanContract>> GetLoansByUserIdAsync(string userId, CancellationToken ct = default);
    Task<LoanContract?> GetLoanByIdAsync(Guid contractId, CancellationToken ct = default);
    Task<IReadOnlyList<LoanPayment>> GetLoanPaymentsAsync(Guid contractId, CancellationToken ct = default);
    Task<IReadOnlyList<LoanInstalment>> GetLoanInstalmentsAsync(Guid contractId, CancellationToken ct = default);
    Task<IReadOnlyList<LoanWarranty>> GetLoanWarrantiesAsync(Guid contractId, CancellationToken ct = default);
    Task<IReadOnlyList<FinancingContract>> GetFinancingsByUserIdAsync(string userId, CancellationToken ct = default);
    Task<FinancingContract?> GetFinancingByIdAsync(Guid contractId, CancellationToken ct = default);
    Task<IReadOnlyList<OverdraftContract>> GetOverdraftsByUserIdAsync(string userId, CancellationToken ct = default);
    Task<OverdraftContract?> GetOverdraftByIdAsync(Guid contractId, CancellationToken ct = default);
    Task AddLoanAsync(LoanContract loan, CancellationToken ct = default);
    Task AddFinancingAsync(FinancingContract financing, CancellationToken ct = default);
    Task AddOverdraftAsync(OverdraftContract overdraft, CancellationToken ct = default);
}
