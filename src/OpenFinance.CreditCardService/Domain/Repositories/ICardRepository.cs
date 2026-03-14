using OpenFinance.CreditCardService.Domain.Entities;

namespace OpenFinance.CreditCardService.Domain.Repositories;

public interface ICardRepository
{
    Task<IReadOnlyList<CardAccount>> GetCardAccountsByUserIdAsync(string userId, CancellationToken ct = default);
    Task<CardAccount?> GetCardAccountByIdAsync(Guid cardAccountId, CancellationToken ct = default);
    Task<IReadOnlyList<CardLimit>> GetCardLimitsAsync(Guid cardAccountId, CancellationToken ct = default);
    Task<IReadOnlyList<CardBill>> GetCardBillsAsync(Guid cardAccountId, CancellationToken ct = default);
    Task<CardBill?> GetCardBillByIdAsync(Guid billId, CancellationToken ct = default);
    Task<IReadOnlyList<CardTransaction>> GetCardBillTransactionsAsync(Guid cardAccountId, Guid billId, CancellationToken ct = default);
    Task AddCardAccountAsync(CardAccount cardAccount, CancellationToken ct = default);
}
