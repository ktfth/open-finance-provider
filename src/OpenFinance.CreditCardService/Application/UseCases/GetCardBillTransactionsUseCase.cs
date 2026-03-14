using OpenFinance.CreditCardService.Domain.Repositories;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.CreditCardService.Application.UseCases;

public sealed class GetCardBillTransactionsUseCase(ICardRepository repository)
{
    public async Task<Result<CardBillTransactionListResponse>> ExecuteAsync(
        Guid cardAccountId,
        Guid billId,
        Guid consentId,
        CancellationToken ct = default)
    {
        var account = await repository.GetCardAccountByIdAsync(cardAccountId, ct);
        if (account is null)
            return Result.Failure<CardBillTransactionListResponse>($"Card account {cardAccountId} not found.");

        var bill = await repository.GetCardBillByIdAsync(billId, ct);
        if (bill is null || bill.CardAccountId != cardAccountId)
            return Result.Failure<CardBillTransactionListResponse>($"Bill {billId} not found for card account {cardAccountId}.");

        var transactions = await repository.GetCardBillTransactionsAsync(cardAccountId, billId, ct);
        var summaries = transactions
            .OrderByDescending(t => t.TransactionDateTime)
            .Select(t => new CardBillTransactionSummary(
                t.Id,
                t.IdentificationNumber,
                t.LineName,
                t.TransactionName,
                t.BillIdentification,
                t.TransactionType,
                t.Amount,
                t.Currency,
                t.TransactionDateTime,
                t.BillPostDate,
                t.PayeeMCC))
            .ToList();

        return Result.Success(new CardBillTransactionListResponse(summaries));
    }
}
