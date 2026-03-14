using OpenFinance.InvestmentService.Domain.Repositories;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.InvestmentService.Application.UseCases;

public sealed class GetTreasuryBondDetailsUseCase(IInvestmentRepository repository)
{
    public async Task<Result<TreasuryBondDetailsResponse?>> ExecuteAsync(
        Guid investmentId, Guid consentId, CancellationToken ct = default)
    {
        var bond = await repository.GetTreasuryBondByIdAsync(investmentId, ct);
        if (bond is null)
            return Result.Success<TreasuryBondDetailsResponse?>(null);

        var response = new TreasuryBondDetailsResponse(
            bond.Id,
            bond.ProductName,
            bond.BondType,
            bond.PurchaseDate,
            bond.MaturityDate,
            bond.NominalQuantity,
            bond.NominalUnitPrice,
            bond.UpdatedUnitPrice,
            bond.GrossAmount,
            bond.IncomeTax,
            bond.IOFTax,
            bond.NetAmount,
            bond.Currency,
            bond.RateType,
            bond.PurchaseIndexValue
        );

        return Result.Success<TreasuryBondDetailsResponse?>(response);
    }
}
