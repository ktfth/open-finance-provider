using OpenFinance.InvestmentService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.InvestmentService.Application.UseCases;

public sealed class GetFixedIncomeUseCase(IInvestmentRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<FixedIncomeListResponse>> ExecuteAsync(
        string userId, Guid consentId, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.InvestmentsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<FixedIncomeListResponse>(validation.ErrorMessage!);

        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var investments = await repository.GetFixedIncomeByUserIdAsync(userId, ct);
        var summaries = investments.Select(i => new FixedIncomeSummary(
            i.Id,
            i.Type,
            i.ProductName,
            i.Issuer,
            i.MaturityDate,
            i.GrossAmount,
            i.Currency
        )).ToList();

        return Result.Success(new FixedIncomeListResponse(summaries));
    }
}
