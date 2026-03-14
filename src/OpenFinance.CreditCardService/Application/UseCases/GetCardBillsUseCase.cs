using OpenFinance.CreditCardService.Domain.Repositories;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Results;

namespace OpenFinance.CreditCardService.Application.UseCases;

public sealed class GetCardBillsUseCase(ICardRepository repository)
{
    public async Task<Result<CardBillListResponse>> ExecuteAsync(
        Guid cardAccountId,
        Guid consentId,
        CancellationToken ct = default)
    {
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
