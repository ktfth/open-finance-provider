using OpenFinance.InvestmentService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.InvestmentService.Application.UseCases;

public sealed class GetTreasuryBondsUseCase(IInvestmentRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<TreasuryBondListResponse>> ExecuteAsync(
        string userId, Guid consentId, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.InvestmentsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<TreasuryBondListResponse>(validation.ErrorMessage!);

        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var bonds = await repository.GetTreasuryBondsByUserIdAsync(userId, ct);
        var summaries = bonds.Select(b => new TreasuryBondSummary(
            b.Id,
            b.ProductName,
            b.BondType,
            b.MaturityDate,
            b.NominalQuantity * b.NominalUnitPrice,
            b.GrossAmount,
            b.Currency
        )).ToList();

        return Result.Success(new TreasuryBondListResponse(summaries));
    }
}
