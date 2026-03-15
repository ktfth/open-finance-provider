using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.CreditCardService.Domain.Entities;

/// <summary>
/// Represents a monthly credit card bill (fatura).
/// Contains the consolidated charges for a billing cycle.
/// </summary>
public class CardBill : Entity
{
    private CardBill() { }

    public static CardBill Create(
        Guid cardAccountId,
        DateOnly dueDate,
        decimal totalAmount,
        decimal minimumPaymentAmount,
        string currency,
        bool isInstalment = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (totalAmount < 0)
            throw new ArgumentException("Total amount cannot be negative.", nameof(totalAmount));
        if (minimumPaymentAmount < 0)
            throw new ArgumentException("Minimum payment amount cannot be negative.", nameof(minimumPaymentAmount));
        if (minimumPaymentAmount > totalAmount && totalAmount > 0)
            throw new ArgumentException("Minimum payment amount cannot exceed total amount.", nameof(minimumPaymentAmount));

        return new CardBill
        {
            CardAccountId = cardAccountId,
            DueDate = dueDate,
            TotalAmount = totalAmount,
            MinimumPaymentAmount = minimumPaymentAmount,
            Currency = currency,
            IsInstalment = isInstalment,
            Status = CardBillStatus.Open
        };
    }

    public Guid CardAccountId { get; private set; }
    public DateOnly DueDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal MinimumPaymentAmount { get; private set; }
    public string Currency { get; private set; } = default!;
    public bool IsInstalment { get; private set; }
    public CardBillStatus Status { get; private set; }

    public void Close()
    {
        if (Status != CardBillStatus.Open)
            throw new InvalidOperationException($"Cannot close a bill in status {Status}.");
        Status = CardBillStatus.Closed;
        SetUpdated();
    }

    public void MarkPaid()
    {
        if (Status == CardBillStatus.Paid)
            throw new InvalidOperationException("Bill is already paid.");
        Status = CardBillStatus.Paid;
        SetUpdated();
    }

    public void MarkOverdue()
    {
        if (Status != CardBillStatus.Closed)
            throw new InvalidOperationException("Only closed bills can become overdue.");
        Status = CardBillStatus.Overdue;
        SetUpdated();
    }
}
