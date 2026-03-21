using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.InsuranceService.Domain.Entities;

/// <summary>
/// Represents an individual premium payment installment for an insurance policy.
/// Maps to the Open Finance Brasil Insurance Premium Payments resource.
/// </summary>
public class InsurancePremiumPayment : Entity
{
    private InsurancePremiumPayment() { }

    public static InsurancePremiumPayment Create(
        Guid insuranceId,
        DateOnly dueDate,
        decimal amount,
        string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (amount <= 0)
            throw new ArgumentException("Payment amount must be positive.", nameof(amount));

        return new InsurancePremiumPayment
        {
            InsuranceId = insuranceId,
            DueDate = dueDate,
            Amount = amount,
            Currency = currency,
            Status = PremiumPaymentStatus.Pending
        };
    }

    public Guid InsuranceId { get; private set; }
    public DateOnly DueDate { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = default!;
    public PremiumPaymentStatus Status { get; private set; }

    public void MarkPaid()
    {
        if (Status == PremiumPaymentStatus.Paid)
            throw new InvalidOperationException("Premium payment is already paid.");
        Status = PremiumPaymentStatus.Paid;
        SetUpdated();
    }

    public void MarkOverdue()
    {
        if (Status != PremiumPaymentStatus.Pending)
            throw new InvalidOperationException("Only pending premium payments can become overdue.");
        Status = PremiumPaymentStatus.Overdue;
        SetUpdated();
    }

    public void Cancel()
    {
        if (Status == PremiumPaymentStatus.Paid)
            throw new InvalidOperationException("Cannot cancel a paid premium payment.");
        Status = PremiumPaymentStatus.Cancelled;
        SetUpdated();
    }
}
