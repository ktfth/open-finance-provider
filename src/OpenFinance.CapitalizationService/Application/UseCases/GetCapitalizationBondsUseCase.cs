using OpenFinance.CapitalizationService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.CapitalizationService.Application.UseCases;

public sealed class GetCapitalizationBondsUseCase(ICapitalizationRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<CapitalizationBondListResponse>> ExecuteAsync(
        string userId, Guid consentId, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.CapitalizationBondsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<CapitalizationBondListResponse>(validation.ErrorMessage!);

        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var bonds = await repository.GetByUserIdAsync(userId, ct);
        var summaries = bonds.Select(b => new CapitalizationBondSummary(
            b.Id,
            b.BondNumber,
            b.Modality,
            b.ProductName,
            b.CompanyName,
            b.CompanyCnpj,
            b.Status,
            b.Currency
        )).ToList();

        return Result.Success(new CapitalizationBondListResponse(summaries));
    }
}
