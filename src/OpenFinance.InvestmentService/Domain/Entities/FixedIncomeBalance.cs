using OpenFinance.Shared.Domain;

namespace OpenFinance.InvestmentService.Domain.Entities;

/// <summary>
/// Current marked-to-market balance for a fixed income investment position.
/// Updated daily with current value, yield, and applicable taxes.
/// </summary>
public class FixedIncomeBalance : Entity
{
    private FixedIncomeBalance() { }

    public static FixedIncomeBalance Create(
        Guid investmentId,
        DateOnly referenceDate,
        decimal grossAmount,
        decimal incomeTax,
        decimal iofTax,
        decimal purchaseUnitPrice,
        decimal updatedUnitPrice,
        decimal quantity,
        string currency)
    {
        if (grossAmount < 0)
            throw new ArgumentException("Gross amount cannot be negative.", nameof(grossAmount));

        return new FixedIncomeBalance
        {
            InvestmentId = investmentId,
            ReferenceDate = referenceDate,
            GrossAmount = grossAmount,
            IncomeTax = incomeTax,
            IOFTax = iofTax,
            PurchaseUnitPrice = purchaseUnitPrice,
            UpdatedUnitPrice = updatedUnitPrice,
            Quantity = quantity,
            Currency = currency
        };
    }

    public Guid InvestmentId { get; private set; }
    public DateOnly ReferenceDate { get; private set; }
    public decimal GrossAmount { get; private set; }
    public decimal IncomeTax { get; private set; }
    public decimal IOFTax { get; private set; }
    public decimal PurchaseUnitPrice { get; private set; }
    public decimal UpdatedUnitPrice { get; private set; }
    public decimal Quantity { get; private set; }
    public string Currency { get; private set; } = default!;

    public decimal NetAmount => GrossAmount - IncomeTax - IOFTax;
    public decimal Yield => PurchaseUnitPrice > 0
        ? (UpdatedUnitPrice - PurchaseUnitPrice) / PurchaseUnitPrice * 100
        : 0;
}
