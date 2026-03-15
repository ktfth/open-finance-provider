using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.InvestmentService.Domain.Entities;

/// <summary>
/// A movement (purchase, redemption, coupon payment) on a fixed income investment.
/// </summary>
public class FixedIncomeTransaction : Entity
{
    private FixedIncomeTransaction() { }

    public static FixedIncomeTransaction Create(
        Guid investmentId,
        FixedIncomeTransactionType type,
        DateOnly transactionDate,
        decimal quantity,
        decimal unitPrice,
        decimal grossValue,
        decimal taxValue,
        string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        return new FixedIncomeTransaction
        {
            InvestmentId = investmentId,
            Type = type,
            TransactionDate = transactionDate,
            Quantity = quantity,
            UnitPrice = unitPrice,
            GrossValue = grossValue,
            TaxValue = taxValue,
            Currency = currency
        };
    }

    public Guid InvestmentId { get; private set; }
    public FixedIncomeTransactionType Type { get; private set; }
    public DateOnly TransactionDate { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal GrossValue { get; private set; }
    public decimal TaxValue { get; private set; }
    public string Currency { get; private set; } = default!;

    public decimal NetValue => GrossValue - TaxValue;
}
