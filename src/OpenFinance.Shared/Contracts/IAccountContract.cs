namespace OpenFinance.Shared.Contracts;

/// <summary>
/// Defines the contract for account information services provided to bank participants.
/// </summary>
public interface IAccountContract
{
    Task<AccountListResponse> GetAccountsAsync(string userId, Guid consentId, CancellationToken ct = default);
    Task<AccountDetailsResponse?> GetAccountDetailsAsync(Guid accountId, Guid consentId, CancellationToken ct = default);
    Task<BalanceResponse?> GetBalanceAsync(Guid accountId, Guid consentId, CancellationToken ct = default);
    Task<TransactionListResponse> GetTransactionsAsync(Guid accountId, Guid consentId, DateOnly from, DateOnly to, CancellationToken ct = default);
}

public record AccountListResponse(IReadOnlyList<AccountSummary> Accounts);

public record AccountSummary(
    Guid AccountId,
    string AccountNumber,
    string BranchCode,
    AccountType Type,
    string Currency
);

public record AccountDetailsResponse(
    Guid AccountId,
    string AccountNumber,
    string BranchCode,
    AccountType Type,
    string Currency,
    string OwnerName,
    string Cpf
);

public record BalanceResponse(
    Guid AccountId,
    decimal AvailableBalance,
    decimal BlockedBalance,
    string Currency,
    DateTime UpdatedAt
);

public record TransactionListResponse(IReadOnlyList<TransactionSummary> Transactions);

public record TransactionSummary(
    Guid TransactionId,
    DateTime CompletedDateTime,
    decimal Amount,
    string Currency,
    string Type,
    string Description
);

public enum AccountType
{
    Checking,
    Savings,
    Prepaid
}
