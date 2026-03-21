using OpenFinance.InsuranceService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.InsuranceService.Application.UseCases;

/// <summary>
/// Returns all coverage lines for a specific insurance policy.
/// Requires permission: INSURANCE_COVERAGES_READ
/// </summary>
public sealed class GetInsuranceCoveragesUseCase(IInsuranceRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<InsuranceCoverageListResponse>> ExecuteAsync(
        Guid insuranceId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.InsurancesRead], ct);
        if (!validation.IsValid)
            return Result.Failure<InsuranceCoverageListResponse>(validation.ErrorMessage!);

        var insurance = await repository.GetByIdAsync(insuranceId, ct);
        if (insurance is null)
            return Result.Failure<InsuranceCoverageListResponse>($"Insurance policy {insuranceId} not found.");

        var coverages = await repository.GetCoveragesAsync(insuranceId, ct);

        var summaries = coverages
            .OrderByDescending(c => c.IsMainCoverage)
            .ThenBy(c => c.CoverageName)
            .Select(c => new InsuranceCoverageSummary(
                c.Id,
                c.CoverageName,
                c.Type,
                c.InsuredAmount,
                c.DeductibleAmount,
                c.Currency,
                c.IsMainCoverage))
            .ToList();

        return Result.Success(new InsuranceCoverageListResponse(summaries));
    }
}
