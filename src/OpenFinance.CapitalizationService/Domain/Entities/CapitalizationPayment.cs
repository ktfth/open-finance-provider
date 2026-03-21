using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.CapitalizationService.Domain.Entities;

/// <summary>
/// Represents a scheduled payment instalment for a capitalization bond.
/// </summary>
public class CapitalizationPayment : Entity
{
    private CapitalizationPayment() { }

    public static CapitalizationPayment Create(
        Guid bondId,
        DateOnly dueDate,
        decimal amount,
        string currency,
        CapitalizationPaymentStatus status)
    {
        if (bondId == Guid.Empty)
            throw new ArgumentException("Bond id must not be empty.", nameof(bondId));
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amount));

        return new CapitalizationPayment
        {
            BondId = bondId,
            DueDate = dueDate,
            Amount = amount,
            Currency = currency,
            Status = status
        };
    }

    public Guid BondId { get; private set; }
    public DateOnly DueDate { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = default!;
    public CapitalizationPaymentStatus Status { get; private set; }
}
