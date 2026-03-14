using OpenFinance.InvestmentService.Domain.Repositories;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.InvestmentService.Application.UseCases;

public sealed class GetVariableIncomeBalanceUseCase(IInvestmentRepository repository)
{
    public async Task<Result<VariableIncomeBalanceResponse?>> ExecuteAsync(
        Guid investmentId, Guid consentId, CancellationToken ct = default)
    {
        var investment = await repository.GetVariableIncomeByIdAsync(investmentId, ct);
        if (investment is null)
            return Result.Success<VariableIncomeBalanceResponse?>(null);

        var yieldPct = investment.AveragePrice > 0
            ? (investment.CurrentPrice - investment.AveragePrice) / investment.AveragePrice * 100
            : 0;

        var response = new VariableIncomeBalanceResponse(
            investmentId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            investment.Quantity,
            investment.CurrentPrice,
            investment.GrossAmount,
            investment.NetAmount,
            investment.IncomeTax,
            investment.Currency,
            investment.GrossAmount - investment.Quantity * investment.AveragePrice,
            yieldPct
        );

        return Result.Success<VariableIncomeBalanceResponse?>(response);
    }
}
