using OpenFinance.CapitalizationService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.CapitalizationService.Application.UseCases;

public sealed class GetCapitalizationBondDetailsUseCase(ICapitalizationRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<CapitalizationBondDetailsResponse?>> ExecuteAsync(
        Guid bondId, Guid consentId, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.CapitalizationBondsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<CapitalizationBondDetailsResponse?>(validation.ErrorMessage!);

        var bond = await repository.GetByIdAsync(bondId, ct);
        if (bond is null)
            return Result.Success<CapitalizationBondDetailsResponse?>(null);

        var response = new CapitalizationBondDetailsResponse(
            bond.Id,
            bond.BondNumber,
            bond.Modality,
            bond.ProductName,
            bond.CompanyName,
            bond.CompanyCnpj,
            bond.Status,
            bond.ContractDate,
            bond.MaturityDate,
            bond.PaymentCount,
            bond.PaymentAmount,
            bond.PaymentFrequency,
            bond.LatePaymentFine,
            bond.LatePaymentInterest,
            bond.RedemptionPercentage,
            bond.CurrentRedemptionValue,
            bond.PrizeDrawAmount,
            bond.Currency,
            bond.TotalPaidAmount,
            bond.MathematicalReserve,
            bond.SurrenderQuota
        );

        return Result.Success<CapitalizationBondDetailsResponse?>(response);
    }
}
