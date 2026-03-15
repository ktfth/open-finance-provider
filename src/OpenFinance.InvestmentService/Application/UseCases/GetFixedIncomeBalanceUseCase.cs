using OpenFinance.InvestmentService.Domain.Repositories;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.InvestmentService.Application.UseCases;

public sealed class GetFixedIncomeBalanceUseCase(IInvestmentRepository repository)
{
    public async Task<Result<FixedIncomeBalanceResponse?>> ExecuteAsync(
        Guid investmentId, Guid consentId, CancellationToken ct = default)
    {
        var investment = await repository.GetFixedIncomeByIdAsync(investmentId, ct);
        if (investment is null)
            return Result.Success<FixedIncomeBalanceResponse?>(null);

        var balance = await repository.GetFixedIncomeBalanceAsync(investmentId, ct);
        if (balance is null)
            return Result.Success<FixedIncomeBalanceResponse?>(null);

        var response = new FixedIncomeBalanceResponse(
            investmentId,
            balance.ReferenceDate,
            balance.GrossAmount,
            balance.NetAmount,
            balance.IncomeTax,
            balance.IOFTax,
            balance.PurchaseUnitPrice,
            balance.UpdatedUnitPrice,
            balance.Quantity,
            balance.Yield,
            balance.Currency
        );

        return Result.Success<FixedIncomeBalanceResponse?>(response);
    }
}
