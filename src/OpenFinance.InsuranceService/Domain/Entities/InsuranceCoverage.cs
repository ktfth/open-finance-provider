using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.InsuranceService.Domain.Entities;

/// <summary>
/// Represents a coverage line within an insurance policy.
/// Maps to the Open Finance Brasil Insurance Coverages resource.
/// </summary>
public class InsuranceCoverage : Entity
{
    private InsuranceCoverage() { }

    public static InsuranceCoverage Create(
        Guid insuranceId,
        string coverageName,
        CoverageType type,
        decimal insuredAmount,
        decimal deductibleAmount,
        string currency,
        bool isMainCoverage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(coverageName);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (insuredAmount <= 0)
            throw new ArgumentException("Insured amount must be positive.", nameof(insuredAmount));
        if (deductibleAmount < 0)
            throw new ArgumentException("Deductible amount cannot be negative.", nameof(deductibleAmount));

        return new InsuranceCoverage
        {
            InsuranceId = insuranceId,
            CoverageName = coverageName,
            Type = type,
            InsuredAmount = insuredAmount,
            DeductibleAmount = deductibleAmount,
            Currency = currency,
            IsMainCoverage = isMainCoverage
        };
    }

    public Guid InsuranceId { get; private set; }
    public string CoverageName { get; private set; } = default!;
    public CoverageType Type { get; private set; }
    public decimal InsuredAmount { get; private set; }
    public decimal DeductibleAmount { get; private set; }
    public string Currency { get; private set; } = default!;
    public bool IsMainCoverage { get; private set; }
}
