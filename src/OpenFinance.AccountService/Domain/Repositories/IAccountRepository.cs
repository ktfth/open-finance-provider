using OpenFinance.AccountService.Domain.Entities;

namespace OpenFinance.AccountService.Domain.Repositories;

public interface IAccountRepository
{
    Task<IReadOnlyList<Account>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<Account?> GetByIdAsync(Guid accountId, CancellationToken ct = default);
    Task<AccountBalance?> GetBalanceAsync(Guid accountId, CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> GetTransactionsAsync(
        Guid accountId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task AddAsync(Account account, CancellationToken ct = default);
}
