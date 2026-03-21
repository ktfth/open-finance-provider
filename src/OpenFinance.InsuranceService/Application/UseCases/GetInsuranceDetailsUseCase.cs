using OpenFinance.InsuranceService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.InsuranceService.Application.UseCases;

/// <summary>
/// Returns detailed information for a specific insurance policy.
/// Requires permission: INSURANCE_READ
/// </summary>
public sealed class GetInsuranceDetailsUseCase(IInsuranceRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<InsuranceDetailsResponse?>> ExecuteAsync(
        Guid insuranceId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.InsurancesRead], ct);
        if (!validation.IsValid)
            return Result.Failure<InsuranceDetailsResponse?>(validation.ErrorMessage!);

        var insurance = await repository.GetByIdAsync(insuranceId, ct);
        if (insurance is null)
            return Result.Success<InsuranceDetailsResponse?>(null);

        var response = new InsuranceDetailsResponse(
            insurance.Id,
            insurance.Type,
            insurance.ProductName,
            insurance.InsurerName,
            insurance.InsurerCnpj,
            insurance.Status,
            insurance.PolicyNumber,
            insurance.ProposalDate,
            insurance.EffectiveDate,
            insurance.ExpirationDate,
            insurance.InsuredAmount,
            insurance.PremiumAmount,
            insurance.Currency,
            insurance.GracePeriodDays,
            insurance.InsuredCpfCnpj,
            insurance.InsuredName,
            insurance.BeneficiaryName);

        return Result.Success<InsuranceDetailsResponse?>(response);
    }
}
