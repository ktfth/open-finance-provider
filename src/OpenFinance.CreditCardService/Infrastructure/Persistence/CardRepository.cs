using Microsoft.EntityFrameworkCore;
using OpenFinance.CreditCardService.Domain.Entities;
using OpenFinance.CreditCardService.Domain.Repositories;

namespace OpenFinance.CreditCardService.Infrastructure.Persistence;

public class CardRepository(CardDbContext context) : ICardRepository
{
    public async Task<IReadOnlyList<CardAccount>> GetCardAccountsByUserIdAsync(string userId, CancellationToken ct = default) =>
        await context.CardAccounts.Where(a => a.UserId == userId).ToListAsync(ct);

    public async Task<CardAccount?> GetCardAccountByIdAsync(Guid cardAccountId, CancellationToken ct = default) =>
        await context.CardAccounts.FirstOrDefaultAsync(a => a.Id == cardAccountId, ct);

    public async Task<IReadOnlyList<CardLimit>> GetCardLimitsAsync(Guid cardAccountId, CancellationToken ct = default) =>
        await context.CardLimits.Where(l => l.CardAccountId == cardAccountId).ToListAsync(ct);

    public async Task<IReadOnlyList<CardBill>> GetCardBillsAsync(Guid cardAccountId, CancellationToken ct = default) =>
        await context.CardBills
            .Where(b => b.CardAccountId == cardAccountId)
            .OrderByDescending(b => b.DueDate)
            .ToListAsync(ct);

    public async Task<CardBill?> GetCardBillByIdAsync(Guid billId, CancellationToken ct = default) =>
        await context.CardBills.FirstOrDefaultAsync(b => b.Id == billId, ct);

    public async Task<IReadOnlyList<CardTransaction>> GetCardBillTransactionsAsync(
        Guid cardAccountId, Guid billId, CancellationToken ct = default) =>
        await context.CardTransactions
            .Where(t => t.CardAccountId == cardAccountId && t.BillId == billId)
            .OrderByDescending(t => t.TransactionDateTime)
            .ToListAsync(ct);

    public async Task AddCardAccountAsync(CardAccount cardAccount, CancellationToken ct = default)
    {
        await context.CardAccounts.AddAsync(cardAccount, ct);
        await context.SaveChangesAsync(ct);
    }
}
