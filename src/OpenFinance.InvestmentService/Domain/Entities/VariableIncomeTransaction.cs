using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.InvestmentService.Domain.Entities;

/// <summary>
/// A movement (buy, sell, dividend, split) on a variable income investment position.
/// </summary>
public class VariableIncomeTransaction : Entity
{
    private VariableIncomeTransaction() { }

    public static VariableIncomeTransaction Create(
        Guid investmentId,
        VariableIncomeTransactionType type,
        DateOnly transactionDate,
        decimal quantity,
        decimal unitPrice,
        decimal grossValue,
        decimal brokerageFee,
        decimal taxValue,
        string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        return new VariableIncomeTransaction
        {
            InvestmentId = investmentId,
            Type = type,
            TransactionDate = transactionDate,
            Quantity = quantity,
            UnitPrice = unitPrice,
            GrossValue = grossValue,
            BrokerageFee = brokerageFee,
            TaxValue = taxValue,
            Currency = currency
        };
    }

    public Guid InvestmentId { get; private set; }
    public VariableIncomeTransactionType Type { get; private set; }
    public DateOnly TransactionDate { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal GrossValue { get; private set; }
    public decimal BrokerageFee { get; private set; }
    public decimal TaxValue { get; private set; }
    public string Currency { get; private set; } = default!;

    public decimal NetValue => GrossValue - BrokerageFee - TaxValue;
}
