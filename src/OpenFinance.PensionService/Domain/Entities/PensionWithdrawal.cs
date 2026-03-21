using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.PensionService.Domain.Entities;

/// <summary>
/// Records a withdrawal or redemption event from a pension plan.
/// Includes gross amount, withholding tax, and net amount paid to the participant.
/// </summary>
public class PensionWithdrawal : Entity
{
    private PensionWithdrawal() { }

    public static PensionWithdrawal Create(
        Guid pensionId,
        DateOnly withdrawalDate,
        decimal grossAmount,
        decimal taxAmount,
        decimal netAmount,
        string currency,
        WithdrawalType type)
    {
        if (pensionId == Guid.Empty)
            throw new ArgumentException("Pension ID must not be empty.", nameof(pensionId));
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        if (grossAmount <= 0)
            throw new ArgumentException("Gross amount must be positive.", nameof(grossAmount));
        if (taxAmount < 0)
            throw new ArgumentException("Tax amount cannot be negative.", nameof(taxAmount));
        if (netAmount < 0)
            throw new ArgumentException("Net amount cannot be negative.", nameof(netAmount));

        return new PensionWithdrawal
        {
            PensionId = pensionId,
            WithdrawalDate = withdrawalDate,
            GrossAmount = grossAmount,
            TaxAmount = taxAmount,
            NetAmount = netAmount,
            Currency = currency,
            Type = type
        };
    }

    public Guid PensionId { get; private set; }
    public DateOnly WithdrawalDate { get; private set; }
    public decimal GrossAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal NetAmount { get; private set; }
    public string Currency { get; private set; } = default!;
    public WithdrawalType Type { get; private set; }
}
