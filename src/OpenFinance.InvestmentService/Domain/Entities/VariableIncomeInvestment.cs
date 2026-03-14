using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.InvestmentService.Domain.Entities;

/// <summary>
/// Represents a variable income investment position (stocks, equity funds, BDRs, ETFs, FIIs).
/// Aligned with the Open Finance Brasil Variable Income specification.
/// </summary>
public class VariableIncomeInvestment : Entity
{
    private VariableIncomeInvestment() { }

    public static VariableIncomeInvestment Create(
        string userId,
        VariableIncomeType type,
        string ticker,
        string productName,
        string isin,
        decimal quantity,
        decimal averagePrice,
        decimal currentPrice,
        string currency,
        DateOnly lastQuoteDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(ticker);
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        if (averagePrice <= 0)
            throw new ArgumentException("Average price must be positive.", nameof(averagePrice));

        return new VariableIncomeInvestment
        {
            UserId = userId,
            Type = type,
            Ticker = ticker,
            ProductName = productName,
            ISIN = isin,
            Quantity = quantity,
            AveragePrice = averagePrice,
            CurrentPrice = currentPrice,
            Currency = currency,
            LastQuoteDate = lastQuoteDate
        };
    }

    public string UserId { get; private set; } = default!;
    public VariableIncomeType Type { get; private set; }
    public string Ticker { get; private set; } = default!;
    public string ProductName { get; private set; } = default!;
    public string ISIN { get; private set; } = default!;
    public decimal Quantity { get; private set; }
    public decimal AveragePrice { get; private set; }
    public decimal CurrentPrice { get; private set; }
    public string Currency { get; private set; } = default!;
    public DateOnly LastQuoteDate { get; private set; }

    public decimal GrossAmount => Quantity * CurrentPrice;
    public decimal IncomeTax => Math.Max(0, (GrossAmount - Quantity * AveragePrice) * 0.15m);
    public decimal NetAmount => GrossAmount - IncomeTax;

    public void UpdatePrice(decimal newPrice, DateOnly quoteDate)
    {
        if (newPrice <= 0)
            throw new ArgumentException("Price must be positive.", nameof(newPrice));
        CurrentPrice = newPrice;
        LastQuoteDate = quoteDate;
        SetUpdated();
    }
}
