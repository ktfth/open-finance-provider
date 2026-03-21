using OpenFinance.CreditCardService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.CreditCardService.Application.UseCases;

public sealed class GetCardBillsUseCase(ICardRepository repository, IConsentValidator consentValidator)
{
    public async Task<Result<CardBillListResponse>> ExecuteAsync(
        Guid cardAccountId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var validation = await consentValidator.ValidateAsync(
            consentId, [OpenFinancePermissions.CreditCardsAccountsBillsRead], ct);
        if (!validation.IsValid)
            return Result.Failure<CardBillListResponse>(validation.ErrorMessage!);

        var account = await repository.GetCardAccountByIdAsync(cardAccountId, ct);
        if (account is null)
            return Result.Failure<CardBillListResponse>($"Card account {cardAccountId} not found.");

        var bills = await repository.GetCardBillsAsync(cardAccountId, ct);
        var summaries = bills
            .OrderByDescending(b => b.DueDate)
            .Select(b => new CardBillSummary(
                b.Id,
                b.DueDate,
                b.TotalAmount,
                b.Currency,
                b.MinimumPaymentAmount,
                b.Currency,
                b.IsInstalment,
                b.Status))
            .ToList();

        return Result.Success(new CardBillListResponse(summaries));
    }
}
