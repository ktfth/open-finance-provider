using OpenFinance.CreditCardService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.CreditCardService.Application.UseCases;

public sealed class GetCardAccountsUseCase(ICardRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<CardAccountListResponse>> ExecuteAsync(
        string userId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.CreditCardsAccountsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<CardAccountListResponse>(validation.ErrorMessage!);

        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var accounts = await repository.GetCardAccountsByUserIdAsync(userId, ct);
        var activeAccounts = accounts
            .Where(a => a.IsActive())
            .Select(a => new CardAccountSummary(
                a.Id,
                a.LastFourDigits,
                a.Brand,
                a.CardType,
                a.HolderName,
                a.Status))
            .ToList();

        return Result.Success(new CardAccountListResponse(activeAccounts));
    }
}
