using OpenFinance.InvestmentService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.InvestmentService.Application.UseCases;

public sealed class GetFixedIncomeDetailsUseCase(IInvestmentRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<FixedIncomeDetailsResponse?>> ExecuteAsync(
        Guid investmentId, Guid consentId, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.InvestmentsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<FixedIncomeDetailsResponse?>(validation.ErrorMessage!);

        var investment = await repository.GetFixedIncomeByIdAsync(investmentId, ct);
        if (investment is null)
            return Result.Success<FixedIncomeDetailsResponse?>(null);

        var response = new FixedIncomeDetailsResponse(
            investment.Id,
            investment.Type,
            investment.ProductName,
            investment.Issuer,
            investment.ISIN,
            investment.IssueDate,
            investment.MaturityDate,
            investment.FaceValue,
            investment.PurchaseUnitPrice,
            investment.Quantity,
            investment.GrossAmount,
            investment.GrossAmount * (1 - investment.TaxExemptionPercentage / 100m),
            investment.Currency,
            investment.Indexer,
            investment.IndexerPercentage,
            investment.PreFixedRate,
            investment.PostFixedRate,
            investment.TaxExemptionPercentage,
            investment.RemunerationType
        );

        return Result.Success<FixedIncomeDetailsResponse?>(response);
    }
}
