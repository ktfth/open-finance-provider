using OpenFinance.AccountService.Domain.Entities;
using OpenFinance.AccountService.Domain.Repositories;

namespace OpenFinance.BDD.Tests.Support;

public class InMemoryAccountRepository : IAccountRepository
{
    private readonly Dictionary<Guid, Account> _accounts = [];
    private readonly Dictionary<Guid, AccountBalance> _balances = [];
    private readonly List<Transaction> _transactions = [];

    public Task<IReadOnlyList<Account>> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Account>>(
            _accounts.Values.Where(a => a.UserId == userId).ToList());

    public Task<Account?> GetByIdAsync(Guid accountId, CancellationToken ct = default) =>
        Task.FromResult(_accounts.GetValueOrDefault(accountId));

    public Task<AccountBalance?> GetBalanceAsync(Guid accountId, CancellationToken ct = default) =>
        Task.FromResult(_balances.GetValueOrDefault(accountId));

    public Task<IReadOnlyList<Transaction>> GetTransactionsAsync(
        Guid accountId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var fromDate = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toDate = to.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var result = _transactions
            .Where(t => t.AccountId == accountId
                     && t.CompletedDateTime >= fromDate
                     && t.CompletedDateTime <= toDate)
            .ToList();

        return Task.FromResult<IReadOnlyList<Transaction>>(result);
    }

    public Task AddAsync(Account account, CancellationToken ct = default)
    {
        _accounts[account.Id] = account;
        return Task.CompletedTask;
    }

    public void AddTransaction(Transaction transaction) => _transactions.Add(transaction);
}
