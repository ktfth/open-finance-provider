using OpenFinance.Shared.Domain;

namespace OpenFinance.AccountService.Domain.Entities;

public class Transaction : Entity
{
    private Transaction() { }

    public static Transaction Create(
        Guid accountId,
        decimal amount,
        string currency,
        string type,
        string description,
        DateTime completedDateTime)
    {
        if (amount == 0)
            throw new ArgumentException("Amount cannot be zero.", nameof(amount));
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        return new Transaction
        {
            AccountId = accountId,
            Amount = amount,
            Currency = currency,
            Type = type,
            Description = description,
            CompletedDateTime = completedDateTime
        };
    }

    public Guid AccountId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = default!;
    public string Type { get; private set; } = default!;
    public string Description { get; private set; } = string.Empty;
    public DateTime CompletedDateTime { get; private set; }
}
