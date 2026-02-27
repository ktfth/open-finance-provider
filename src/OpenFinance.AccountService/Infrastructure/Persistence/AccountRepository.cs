using Microsoft.EntityFrameworkCore;
using OpenFinance.AccountService.Domain.Entities;
using OpenFinance.AccountService.Domain.Repositories;

namespace OpenFinance.AccountService.Infrastructure.Persistence;

public class AccountRepository(AccountDbContext context) : IAccountRepository
{
    public async Task<IReadOnlyList<Account>> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        await context.Accounts.Where(a => a.UserId == userId).ToListAsync(ct);

    public async Task<Account?> GetByIdAsync(Guid accountId, CancellationToken ct = default) =>
        await context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, ct);

    public async Task<AccountBalance?> GetBalanceAsync(Guid accountId, CancellationToken ct = default) =>
        await context.AccountBalances.FirstOrDefaultAsync(b => b.AccountId == accountId, ct);

    public async Task<IReadOnlyList<Transaction>> GetTransactionsAsync(
        Guid accountId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var fromDate = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toDate = to.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return await context.Transactions
            .Where(t => t.AccountId == accountId
                     && t.CompletedDateTime >= fromDate
                     && t.CompletedDateTime <= toDate)
            .OrderByDescending(t => t.CompletedDateTime)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Account account, CancellationToken ct = default)
    {
        await context.Accounts.AddAsync(account, ct);
        await context.SaveChangesAsync(ct);
    }
}
