using OpenFinance.InvestmentService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.InvestmentService.Application.UseCases;

public sealed class GetTreasuryBondBalanceUseCase(IInvestmentRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<TreasuryBondBalanceResponse?>> ExecuteAsync(
        Guid investmentId, Guid consentId, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.InvestmentsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<TreasuryBondBalanceResponse?>(validation.ErrorMessage!);

        var bond = await repository.GetTreasuryBondByIdAsync(investmentId, ct);
        if (bond is null)
            return Result.Success<TreasuryBondBalanceResponse?>(null);

        var purchaseCost = bond.NominalQuantity * bond.NominalUnitPrice;
        var yield = purchaseCost > 0 ? bond.GrossAmount - purchaseCost : 0;

        var response = new TreasuryBondBalanceResponse(
            bond.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            bond.GrossAmount,
            bond.NetAmount,
            bond.IncomeTax,
            bond.IOFTax,
            yield,
            bond.UpdatedUnitPrice,
            bond.Currency
        );

        return Result.Success<TreasuryBondBalanceResponse?>(response);
    }
}
