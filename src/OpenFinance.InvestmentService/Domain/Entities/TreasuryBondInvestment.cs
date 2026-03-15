using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.InvestmentService.Domain.Entities;

/// <summary>
/// Represents a Tesouro Direto (Brazilian Government Treasury Bond) position.
/// Covers Tesouro Prefixado, Tesouro IPCA+, Tesouro Selic and their variants.
/// Aligned with the Open Finance Brasil Treasury Bonds specification.
/// </summary>
public class TreasuryBondInvestment : Entity
{
    private TreasuryBondInvestment() { }

    public static TreasuryBondInvestment Create(
        string userId,
        string productName,
        TreasuryBondType bondType,
        DateOnly purchaseDate,
        DateOnly maturityDate,
        decimal nominalQuantity,
        decimal nominalUnitPrice,
        decimal updatedUnitPrice,
        string currency,
        decimal rateType,
        decimal purchaseIndexValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (nominalQuantity <= 0)
            throw new ArgumentException("Nominal quantity must be positive.", nameof(nominalQuantity));
        if (nominalUnitPrice <= 0)
            throw new ArgumentException("Nominal unit price must be positive.", nameof(nominalUnitPrice));
        if (maturityDate <= purchaseDate)
            throw new ArgumentException("Maturity date must be after purchase date.", nameof(maturityDate));

        return new TreasuryBondInvestment
        {
            UserId = userId,
            ProductName = productName,
            BondType = bondType,
            PurchaseDate = purchaseDate,
            MaturityDate = maturityDate,
            NominalQuantity = nominalQuantity,
            NominalUnitPrice = nominalUnitPrice,
            UpdatedUnitPrice = updatedUnitPrice,
            Currency = currency,
            RateType = rateType,
            PurchaseIndexValue = purchaseIndexValue
        };
    }

    public string UserId { get; private set; } = default!;
    public string ProductName { get; private set; } = default!;
    public TreasuryBondType BondType { get; private set; }
    public DateOnly PurchaseDate { get; private set; }
    public DateOnly MaturityDate { get; private set; }
    public decimal NominalQuantity { get; private set; }
    public decimal NominalUnitPrice { get; private set; }
    public decimal UpdatedUnitPrice { get; private set; }
    public string Currency { get; private set; } = default!;
    public decimal RateType { get; private set; }
    public decimal PurchaseIndexValue { get; private set; }

    public decimal GrossAmount => NominalQuantity * UpdatedUnitPrice;
    /// <summary>Brazilian IR on Tesouro Direto is regressive: 15% after 720 days</summary>
    public decimal IncomeTax => Math.Max(0, (GrossAmount - NominalQuantity * NominalUnitPrice) * GetIncomeTaxRate());
    /// <summary>IOF applies only for redemptions before 30 days</summary>
    public decimal IOFTax => 0m;
    public decimal NetAmount => GrossAmount - IncomeTax - IOFTax;

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new ArgumentException("Price must be positive.", nameof(newPrice));
        UpdatedUnitPrice = newPrice;
        SetUpdated();
    }

    private decimal GetIncomeTaxRate()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var days = today.DayNumber - PurchaseDate.DayNumber;
        return days switch
        {
            <= 180 => 0.225m,
            <= 360 => 0.20m,
            <= 720 => 0.175m,
            _ => 0.15m
        };
    }
}
