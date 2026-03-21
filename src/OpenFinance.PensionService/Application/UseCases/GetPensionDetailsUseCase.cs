using OpenFinance.PensionService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.PensionService.Application.UseCases;

/// <summary>
/// Returns the full details of a specific pension plan, including tax regime,
/// beneficiary, fee structure, and scheduled retirement date.
/// </summary>
public sealed class GetPensionDetailsUseCase(IPensionRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<PensionDetailsResponse?>> ExecuteAsync(
        Guid pensionId, Guid consentId, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.PensionsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<PensionDetailsResponse?>(validation.ErrorMessage!);

        var pension = await repository.GetByIdAsync(pensionId, ct);
        if (pension is null)
            return Result.Success<PensionDetailsResponse?>(null);

        var response = new PensionDetailsResponse(
            pension.Id,
            pension.Type,
            pension.Modality,
            pension.ProductName,
            pension.InsurerName,
            pension.InsurerCnpj,
            pension.Status,
            pension.CertificateNumber,
            pension.ContractDate,
            pension.RetirementDate,
            pension.TaxRegime,
            pension.ContributionAmount,
            pension.ContributionFrequency,
            pension.Currency,
            pension.BeneficiaryName,
            pension.ManagementFeeRate,
            pension.LoadingRate,
            pension.IncomeType
        );

        return Result.Success<PensionDetailsResponse?>(response);
    }
}
