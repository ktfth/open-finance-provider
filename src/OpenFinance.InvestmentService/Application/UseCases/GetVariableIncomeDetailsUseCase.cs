using OpenFinance.InvestmentService.Domain.Repositories;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.InvestmentService.Application.UseCases;

public sealed class GetVariableIncomeDetailsUseCase(IInvestmentRepository repository)
{
    public async Task<Result<VariableIncomeDetailsResponse?>> ExecuteAsync(
        Guid investmentId, Guid consentId, CancellationToken ct = default)
    {
        var investment = await repository.GetVariableIncomeByIdAsync(investmentId, ct);
        if (investment is null)
            return Result.Success<VariableIncomeDetailsResponse?>(null);

        var response = new VariableIncomeDetailsResponse(
            investment.Id,
            investment.Type,
            investment.Ticker,
            investment.ProductName,
            investment.ISIN,
            investment.Quantity,
            investment.AveragePrice,
            investment.CurrentPrice,
            investment.GrossAmount,
            investment.NetAmount,
            investment.IncomeTax,
            investment.Currency,
            investment.LastQuoteDate
        );

        return Result.Success<VariableIncomeDetailsResponse?>(response);
    }
}
