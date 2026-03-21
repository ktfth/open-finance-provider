using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.PensionService.Domain.Entities;

/// <summary>
/// Records a single contribution (aporte) made to a pension plan.
/// Covers regular, extra, portability-in, and employer contributions.
/// </summary>
public class PensionContribution : Entity
{
    private PensionContribution() { }

    public static PensionContribution Create(
        Guid pensionId,
        DateOnly contributionDate,
        decimal amount,
        string currency,
        ContributionType type,
        ContributionStatus status)
    {
        if (pensionId == Guid.Empty)
            throw new ArgumentException("Pension ID must not be empty.", nameof(pensionId));
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        if (amount <= 0)
            throw new ArgumentException("Contribution amount must be positive.", nameof(amount));

        return new PensionContribution
        {
            PensionId = pensionId,
            ContributionDate = contributionDate,
            Amount = amount,
            Currency = currency,
            Type = type,
            Status = status
        };
    }

    public Guid PensionId { get; private set; }
    public DateOnly ContributionDate { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = default!;
    public ContributionType Type { get; private set; }
    public ContributionStatus Status { get; private set; }
}
