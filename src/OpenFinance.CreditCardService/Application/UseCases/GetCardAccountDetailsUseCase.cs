using OpenFinance.CreditCardService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.CreditCardService.Application.UseCases;

public sealed class GetCardAccountDetailsUseCase(ICardRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<CardAccountDetailsResponse?>> ExecuteAsync(
        Guid cardAccountId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.CreditCardsAccountsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<CardAccountDetailsResponse?>(validation.ErrorMessage!);

        var account = await repository.GetCardAccountByIdAsync(cardAccountId, ct);
        if (account is null)
            return Result.Success<CardAccountDetailsResponse?>(null);

        var dueDate = DateTime.UtcNow.AddDays(
            account.PaymentDay >= DateTime.UtcNow.Day
                ? account.PaymentDay - DateTime.UtcNow.Day
                : 30 - DateTime.UtcNow.Day + account.PaymentDay);

        var response = new CardAccountDetailsResponse(
            account.Id,
            account.LastFourDigits,
            account.Brand,
            account.CardType,
            account.NetworkType,
            account.HolderName,
            account.HolderCpf,
            account.Status,
            dueDate,
            account.PaymentDay);

        return Result.Success<CardAccountDetailsResponse?>(response);
    }
}
