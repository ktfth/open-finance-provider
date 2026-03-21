using OpenFinance.CreditCardService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.CreditCardService.Application.UseCases;

public sealed class GetCardLimitsUseCase(ICardRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<CardLimitsResponse?>> ExecuteAsync(
        Guid cardAccountId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.CreditCardsAccountsLimitsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<CardLimitsResponse?>(validation.ErrorMessage!);

        var account = await repository.GetCardAccountByIdAsync(cardAccountId, ct);
        if (account is null)
            return Result.Success<CardLimitsResponse?>(null);

        var limits = await repository.GetCardLimitsAsync(cardAccountId, ct);

        var limitDtos = limits.Select(l => new CreditLimit(
            l.LimitType,
            l.CreditLineLimitType,
            l.ConsolidationType,
            l.IdentificationNumber,
            l.LineName,
            string.Empty,
            l.IsLimitFlexible,
            l.LimitAmountTotal,
            l.Currency,
            l.UsedAmountTotal,
            l.Currency,
            l.AvailableAmountTotal,
            l.Currency
        )).ToList();

        return Result.Success<CardLimitsResponse?>(new CardLimitsResponse(cardAccountId, limitDtos));
    }
}
