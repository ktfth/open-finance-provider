using OpenFinance.Shared.Domain;

namespace OpenFinance.AccountService.Domain.Entities;

public class AccountBalance : Entity
{
    private AccountBalance() { }

    public static AccountBalance Create(Guid accountId, decimal available, decimal blocked, string currency)
    {
        if (available < 0)
            throw new ArgumentException("Available balance cannot be negative.", nameof(available));
        if (blocked < 0)
            throw new ArgumentException("Blocked balance cannot be negative.", nameof(blocked));
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        return new AccountBalance
        {
            AccountId = accountId,
            AvailableBalance = available,
            BlockedBalance = blocked,
            Currency = currency,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Guid AccountId { get; private set; }
    public decimal AvailableBalance { get; private set; }
    public decimal BlockedBalance { get; private set; }
    public string Currency { get; private set; } = default!;
    public new DateTime UpdatedAt { get; private set; }
}
