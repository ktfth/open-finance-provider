using OpenFinance.InsuranceService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.InsuranceService.Application.UseCases;

/// <summary>
/// Returns all claims filed against a specific insurance policy.
/// Requires permission: INSURANCE_CLAIMS_READ
/// </summary>
public sealed class GetInsuranceClaimsUseCase(IInsuranceRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<InsuranceClaimListResponse>> ExecuteAsync(
        Guid insuranceId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.InsurancesRead], ct);
        if (!validation.IsValid)
            return Result.Failure<InsuranceClaimListResponse>(validation.ErrorMessage!);

        var insurance = await repository.GetByIdAsync(insuranceId, ct);
        if (insurance is null)
            return Result.Failure<InsuranceClaimListResponse>($"Insurance policy {insuranceId} not found.");

        var claims = await repository.GetClaimsAsync(insuranceId, ct);

        var summaries = claims
            .OrderByDescending(c => c.NotificationDate)
            .Select(c => new InsuranceClaimSummary(
                c.Id,
                c.ClaimNumber,
                c.OccurrenceDate,
                c.NotificationDate,
                c.ClaimedAmount,
                c.ApprovedAmount,
                c.Currency,
                c.Status))
            .ToList();

        return Result.Success(new InsuranceClaimListResponse(summaries));
    }
}
