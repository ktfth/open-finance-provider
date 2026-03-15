using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.CreditCardService.Domain.Entities;

/// <summary>
/// Represents a credit limit line associated with a card account.
/// A single card account can have multiple limit lines (e.g., total, national, international).
/// </summary>
public class CardLimit : Entity
{
    private CardLimit() { }

    public static CardLimit Create(
        Guid cardAccountId,
        CreditLimitType limitType,
        string creditLineLimitType,
        string consolidationType,
        string identificationNumber,
        string lineName,
        bool isLimitFlexible,
        decimal limitAmountTotal,
        string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(creditLineLimitType);
        ArgumentException.ThrowIfNullOrWhiteSpace(consolidationType);
        ArgumentException.ThrowIfNullOrWhiteSpace(identificationNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(lineName);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (limitAmountTotal < 0)
            throw new ArgumentException("Limit amount cannot be negative.", nameof(limitAmountTotal));

        return new CardLimit
        {
            CardAccountId = cardAccountId,
            LimitType = limitType,
            CreditLineLimitType = creditLineLimitType,
            ConsolidationType = consolidationType,
            IdentificationNumber = identificationNumber,
            LineName = lineName,
            IsLimitFlexible = isLimitFlexible,
            LimitAmountTotal = limitAmountTotal,
            UsedAmountTotal = 0,
            Currency = currency
        };
    }

    public Guid CardAccountId { get; private set; }
    public CreditLimitType LimitType { get; private set; }
    public string CreditLineLimitType { get; private set; } = default!;
    public string ConsolidationType { get; private set; } = default!;
    public string IdentificationNumber { get; private set; } = default!;
    public string LineName { get; private set; } = default!;
    public bool IsLimitFlexible { get; private set; }
    public decimal LimitAmountTotal { get; private set; }
    public decimal UsedAmountTotal { get; private set; }
    public string Currency { get; private set; } = default!;

    public decimal AvailableAmountTotal => LimitAmountTotal - UsedAmountTotal;

    public void UpdateUsage(decimal usedAmount)
    {
        if (usedAmount < 0)
            throw new ArgumentException("Used amount cannot be negative.", nameof(usedAmount));
        UsedAmountTotal = usedAmount;
        SetUpdated();
    }
}
