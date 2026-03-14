using OpenFinance.InvestmentService.Domain.Entities;

namespace OpenFinance.InvestmentService.Domain.Repositories;

public interface IInvestmentRepository
{
    // Fixed Income
    Task<IReadOnlyList<FixedIncomeInvestment>> GetFixedIncomeByUserIdAsync(string userId, CancellationToken ct = default);
    Task<FixedIncomeInvestment?> GetFixedIncomeByIdAsync(Guid investmentId, CancellationToken ct = default);
    Task<FixedIncomeBalance?> GetFixedIncomeBalanceAsync(Guid investmentId, CancellationToken ct = default);
    Task<IReadOnlyList<FixedIncomeTransaction>> GetFixedIncomeTransactionsAsync(Guid investmentId, DateOnly from, DateOnly to, CancellationToken ct = default);

    // Variable Income
    Task<IReadOnlyList<VariableIncomeInvestment>> GetVariableIncomeByUserIdAsync(string userId, CancellationToken ct = default);
    Task<VariableIncomeInvestment?> GetVariableIncomeByIdAsync(Guid investmentId, CancellationToken ct = default);
    Task<IReadOnlyList<VariableIncomeTransaction>> GetVariableIncomeTransactionsAsync(Guid investmentId, DateOnly from, DateOnly to, CancellationToken ct = default);

    // Treasury Bonds
    Task<IReadOnlyList<TreasuryBondInvestment>> GetTreasuryBondsByUserIdAsync(string userId, CancellationToken ct = default);
    Task<TreasuryBondInvestment?> GetTreasuryBondByIdAsync(Guid investmentId, CancellationToken ct = default);
}
