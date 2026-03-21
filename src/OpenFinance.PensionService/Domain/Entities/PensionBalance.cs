using OpenFinance.Shared.Domain;

namespace OpenFinance.PensionService.Domain.Entities;

/// <summary>
/// Represents the current financial position of a pension plan at a given reference date.
/// Includes gross/net balances, accumulated contributions, yield, and fees charged.
/// </summary>
public class PensionBalance : Entity
{
    private PensionBalance() { }

    public static PensionBalance Create(
        Guid pensionId,
        DateOnly referenceDate,
        decimal grossBalance,
        decimal netBalance,
        decimal totalContributions,
        decimal totalYield,
        decimal managementFee,
        decimal loadingFee,
        string currency)
    {
        if (pensionId == Guid.Empty)
            throw new ArgumentException("Pension ID must not be empty.", nameof(pensionId));
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        if (grossBalance < 0)
            throw new ArgumentException("Gross balance cannot be negative.", nameof(grossBalance));
        if (netBalance < 0)
            throw new ArgumentException("Net balance cannot be negative.", nameof(netBalance));

        return new PensionBalance
        {
            PensionId = pensionId,
            ReferenceDate = referenceDate,
            GrossBalance = grossBalance,
            NetBalance = netBalance,
            TotalContributions = totalContributions,
            TotalYield = totalYield,
            ManagementFee = managementFee,
            LoadingFee = loadingFee,
            Currency = currency
        };
    }

    public Guid PensionId { get; private set; }
    public DateOnly ReferenceDate { get; private set; }
    public decimal GrossBalance { get; private set; }
    public decimal NetBalance { get; private set; }
    public decimal TotalContributions { get; private set; }
    public decimal TotalYield { get; private set; }
    public decimal ManagementFee { get; private set; }
    public decimal LoadingFee { get; private set; }
    public string Currency { get; private set; } = default!;
}
