using OpenFinance.InvestmentService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.InvestmentService.Application.UseCases;

public sealed class GetVariableIncomeUseCase(IInvestmentRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<VariableIncomeListResponse>> ExecuteAsync(
        string userId, Guid consentId, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.InvestmentsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<VariableIncomeListResponse>(validation.ErrorMessage!);

        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var investments = await repository.GetVariableIncomeByUserIdAsync(userId, ct);
        var summaries = investments.Select(i => new VariableIncomeSummary(
            i.Id,
            i.Type,
            i.Ticker,
            i.ProductName,
            i.Quantity,
            i.GrossAmount,
            i.Currency
        )).ToList();

        return Result.Success(new VariableIncomeListResponse(summaries));
    }
}
