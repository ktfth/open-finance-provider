using OpenFinance.InvestmentService.Domain.Repositories;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.InvestmentService.Application.UseCases;

public sealed class GetFixedIncomeTransactionsUseCase(IInvestmentRepository repository)
{
    public async Task<Result<FixedIncomeTransactionListResponse>> ExecuteAsync(
        Guid investmentId, Guid consentId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        if (from > to)
            return Result.Failure<FixedIncomeTransactionListResponse>("'from' date must be before or equal to 'to' date.");

        var investment = await repository.GetFixedIncomeByIdAsync(investmentId, ct);
        if (investment is null)
            return Result.Failure<FixedIncomeTransactionListResponse>($"Investment {investmentId} not found.");

        var transactions = await repository.GetFixedIncomeTransactionsAsync(investmentId, from, to, ct);
        var summaries = transactions.Select(t => new FixedIncomeTransactionSummary(
            t.Id,
            t.Type,
            t.TransactionDate,
            t.Quantity,
            t.UnitPrice,
            t.GrossValue,
            t.TaxValue,
            t.NetValue,
            t.Currency
        )).ToList();

        return Result.Success(new FixedIncomeTransactionListResponse(summaries));
    }
}
