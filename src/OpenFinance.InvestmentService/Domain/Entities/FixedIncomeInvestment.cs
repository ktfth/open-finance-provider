using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.InvestmentService.Domain.Entities;

/// <summary>
/// Represents a fixed income investment position (CDB, LCI, LCA, CRI, CRA, Debêntures, LF).
/// Aligned with the Open Finance Brasil Fixed Income specification.
/// </summary>
public class FixedIncomeInvestment : Entity
{
    private FixedIncomeInvestment() { }

    public static FixedIncomeInvestment Create(
        string userId,
        FixedIncomeType type,
        string productName,
        string issuer,
        string isin,
        DateOnly issueDate,
        DateOnly maturityDate,
        decimal faceValue,
        decimal purchaseUnitPrice,
        decimal quantity,
        string currency,
        RateIndexer indexer,
        decimal indexerPercentage,
        decimal preFixedRate,
        decimal postFixedRate,
        decimal taxExemptionPercentage,
        RemunType remunerationType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        if (purchaseUnitPrice <= 0)
            throw new ArgumentException("Purchase unit price must be positive.", nameof(purchaseUnitPrice));
        if (maturityDate <= issueDate)
            throw new ArgumentException("Maturity date must be after issue date.", nameof(maturityDate));

        return new FixedIncomeInvestment
        {
            UserId = userId,
            Type = type,
            ProductName = productName,
            Issuer = issuer,
            ISIN = isin,
            IssueDate = issueDate,
            MaturityDate = maturityDate,
            FaceValue = faceValue,
            PurchaseUnitPrice = purchaseUnitPrice,
            Quantity = quantity,
            Currency = currency,
            Indexer = indexer,
            IndexerPercentage = indexerPercentage,
            PreFixedRate = preFixedRate,
            PostFixedRate = postFixedRate,
            TaxExemptionPercentage = taxExemptionPercentage,
            RemunerationType = remunerationType
        };
    }

    public string UserId { get; private set; } = default!;
    public FixedIncomeType Type { get; private set; }
    public string ProductName { get; private set; } = default!;
    public string Issuer { get; private set; } = default!;
    public string ISIN { get; private set; } = default!;
    public DateOnly IssueDate { get; private set; }
    public DateOnly MaturityDate { get; private set; }
    public decimal FaceValue { get; private set; }
    public decimal PurchaseUnitPrice { get; private set; }
    public decimal Quantity { get; private set; }
    public string Currency { get; private set; } = default!;
    public RateIndexer Indexer { get; private set; }
    public decimal IndexerPercentage { get; private set; }
    public decimal PreFixedRate { get; private set; }
    public decimal PostFixedRate { get; private set; }
    public decimal TaxExemptionPercentage { get; private set; }
    public RemunType RemunerationType { get; private set; }

    public decimal GrossAmount => Quantity * PurchaseUnitPrice;
}
