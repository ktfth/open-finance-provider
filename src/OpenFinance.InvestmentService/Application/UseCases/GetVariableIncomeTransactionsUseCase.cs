using OpenFinance.InvestmentService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.InvestmentService.Application.UseCases;

public sealed class GetVariableIncomeTransactionsUseCase(IInvestmentRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<VariableIncomeTransactionListResponse>> ExecuteAsync(
        Guid investmentId, Guid consentId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.InvestmentsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<VariableIncomeTransactionListResponse>(validation.ErrorMessage!);

        if (from > to)
            return Result.Failure<VariableIncomeTransactionListResponse>("'from' date must be before or equal to 'to' date.");

        var investment = await repository.GetVariableIncomeByIdAsync(investmentId, ct);
        if (investment is null)
            return Result.Failure<VariableIncomeTransactionListResponse>($"Investment {investmentId} not found.");

        var transactions = await repository.GetVariableIncomeTransactionsAsync(investmentId, from, to, ct);
        var summaries = transactions.Select(t => new VariableIncomeTransactionSummary(
            t.Id,
            t.Type,
            t.TransactionDate,
            t.Quantity,
            t.UnitPrice,
            t.GrossValue,
            t.BrokerageFee,
            t.TaxValue,
            t.NetValue,
            t.Currency
        )).ToList();

        return Result.Success(new VariableIncomeTransactionListResponse(summaries));
    }
}
