using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.PensionService.Domain.Entities;

/// <summary>
/// Represents a pension plan (previdência complementar) held by a user.
/// Covers PGBL, VGBL, and other supplementary pension products.
/// Aligned with the Open Finance Brasil Pension API specification (Phase 4).
/// </summary>
public class Pension : Entity
{
    private Pension() { }

    public static Pension Create(
        string userId,
        PensionType type,
        PensionModality modality,
        string productName,
        string insurerName,
        string insurerCnpj,
        string certificateNumber,
        DateOnly contractDate,
        DateOnly? retirementDate,
        TaxRegimeType taxRegime,
        decimal contributionAmount,
        PaymentFrequency contributionFrequency,
        string currency,
        string beneficiaryName,
        decimal managementFeeRate,
        decimal loadingRate,
        IncomeType incomeType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentException.ThrowIfNullOrWhiteSpace(insurerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(insurerCnpj);
        ArgumentException.ThrowIfNullOrWhiteSpace(certificateNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        ArgumentException.ThrowIfNullOrWhiteSpace(beneficiaryName);

        if (contributionAmount < 0)
            throw new ArgumentException("Contribution amount cannot be negative.", nameof(contributionAmount));
        if (managementFeeRate < 0)
            throw new ArgumentException("Management fee rate cannot be negative.", nameof(managementFeeRate));
        if (loadingRate < 0)
            throw new ArgumentException("Loading rate cannot be negative.", nameof(loadingRate));

        return new Pension
        {
            UserId = userId,
            Type = type,
            Modality = modality,
            ProductName = productName,
            InsurerName = insurerName,
            InsurerCnpj = insurerCnpj,
            Status = PensionStatus.Active,
            CertificateNumber = certificateNumber,
            ContractDate = contractDate,
            RetirementDate = retirementDate,
            TaxRegime = taxRegime,
            ContributionAmount = contributionAmount,
            ContributionFrequency = contributionFrequency,
            Currency = currency,
            BeneficiaryName = beneficiaryName,
            ManagementFeeRate = managementFeeRate,
            LoadingRate = loadingRate,
            IncomeType = incomeType
        };
    }

    public string UserId { get; private set; } = default!;
    public PensionType Type { get; private set; }
    public PensionModality Modality { get; private set; }
    public string ProductName { get; private set; } = default!;
    public string InsurerName { get; private set; } = default!;
    public string InsurerCnpj { get; private set; } = default!;
    public PensionStatus Status { get; private set; }
    public string CertificateNumber { get; private set; } = default!;
    public DateOnly ContractDate { get; private set; }
    public DateOnly? RetirementDate { get; private set; }
    public TaxRegimeType TaxRegime { get; private set; }
    public decimal ContributionAmount { get; private set; }
    public PaymentFrequency ContributionFrequency { get; private set; }
    public string Currency { get; private set; } = default!;
    public string BeneficiaryName { get; private set; } = default!;
    public decimal ManagementFeeRate { get; private set; }
    public decimal LoadingRate { get; private set; }
    public IncomeType IncomeType { get; private set; }

    public void Suspend()
    {
        if (Status != PensionStatus.Active)
            throw new InvalidOperationException($"Cannot suspend a pension in status {Status}.");
        Status = PensionStatus.Suspended;
        SetUpdated();
    }

    public void Cancel()
    {
        if (Status is PensionStatus.Cancelled or PensionStatus.Redeemed)
            throw new InvalidOperationException($"Cannot cancel a pension in status {Status}.");
        Status = PensionStatus.Cancelled;
        SetUpdated();
    }

    public void Redeem()
    {
        if (Status is PensionStatus.Cancelled or PensionStatus.Redeemed)
            throw new InvalidOperationException($"Cannot redeem a pension in status {Status}.");
        Status = PensionStatus.Redeemed;
        SetUpdated();
    }
}
