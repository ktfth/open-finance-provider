using OpenFinance.CapitalizationService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.CapitalizationService.Application.UseCases;

public sealed class GetCapitalizationBondPaymentsUseCase(ICapitalizationRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<CapitalizationBondPaymentListResponse>> ExecuteAsync(
        Guid bondId, Guid consentId, CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.CapitalizationBondsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<CapitalizationBondPaymentListResponse>(validation.ErrorMessage!);

        var payments = await repository.GetPaymentsAsync(bondId, ct);
        var summaries = payments.Select(p => new CapitalizationBondPaymentSummary(
            p.Id,
            p.DueDate,
            p.Amount,
            p.Currency,
            p.Status
        )).ToList();

        return Result.Success(new CapitalizationBondPaymentListResponse(summaries));
    }
}
