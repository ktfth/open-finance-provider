namespace OpenFinance.Shared.Contracts;

/// <summary>
/// Defines the contract for pension (previdência) data services provided to bank participants.
/// Aligned with the Open Finance Brasil Pension API specification (Phase 4).
/// </summary>
public interface IPensionContract
{
    Task<PensionListResponse> GetPensionsAsync(string userId, Guid consentId, CancellationToken ct = default);
    Task<PensionDetailsResponse?> GetPensionDetailsAsync(Guid pensionId, Guid consentId, CancellationToken ct = default);
    Task<PensionBalanceResponse?> GetPensionBalanceAsync(Guid pensionId, Guid consentId, CancellationToken ct = default);
    Task<PensionContributionListResponse> GetPensionContributionsAsync(Guid pensionId, Guid consentId, CancellationToken ct = default);
    Task<PensionWithdrawalListResponse> GetPensionWithdrawalsAsync(Guid pensionId, Guid consentId, CancellationToken ct = default);
}

public record PensionListResponse(IReadOnlyList<PensionSummary> Pensions);

public record PensionSummary(
    Guid PensionId,
    PensionType Type,
    PensionModality Modality,
    string ProductName,
    string InsurerName,
    string InsurerCnpj,
    PensionStatus Status,
    string CertificateNumber,
    string Currency
);

public record PensionDetailsResponse(
    Guid PensionId,
    PensionType Type,
    PensionModality Modality,
    string ProductName,
    string InsurerName,
    string InsurerCnpj,
    PensionStatus Status,
    string CertificateNumber,
    DateOnly ContractDate,
    DateOnly? RetirementDate,
    TaxRegimeType TaxRegime,
    decimal ContributionAmount,
    PaymentFrequency ContributionFrequency,
    string Currency,
    string BeneficiaryName,
    decimal ManagementFeeRate,
    decimal LoadingRate,
    IncomeType IncomeType
);

public record PensionBalanceResponse(
    Guid PensionId,
    DateOnly ReferenceDate,
    decimal GrossBalance,
    decimal NetBalance,
    decimal TotalContributions,
    decimal TotalYield,
    decimal ManagementFee,
    decimal LoadingFee,
    string Currency
);

public record PensionContributionListResponse(IReadOnlyList<PensionContributionSummary> Contributions);

public record PensionContributionSummary(
    Guid ContributionId,
    DateOnly ContributionDate,
    decimal Amount,
    string Currency,
    ContributionType Type,
    ContributionStatus Status
);

public record PensionWithdrawalListResponse(IReadOnlyList<PensionWithdrawalSummary> Withdrawals);

public record PensionWithdrawalSummary(
    Guid WithdrawalId,
    DateOnly WithdrawalDate,
    decimal GrossAmount,
    decimal TaxAmount,
    decimal NetAmount,
    string Currency,
    WithdrawalType Type
);

// ─── Enumerations ────────────────────────────────────────────────────────────

public enum PensionType
{
    PGBL,
    VGBL,
    PREVI,
    Other
}

public enum PensionModality
{
    Contribution,
    Benefit,
    PensionFund
}

public enum PensionStatus
{
    Active,
    ActiveWithBenefit,
    Suspended,
    Cancelled,
    Redeemed
}

public enum TaxRegimeType
{
    Progressive,
    Regressive
}

public enum IncomeType
{
    LifeAnnuity,
    TemporaryAnnuity,
    LumpSum,
    ScheduledWithdrawal
}

public enum ContributionType
{
    Regular,
    Extra,
    Portability,
    Employer
}

public enum ContributionStatus
{
    Confirmed,
    Pending,
    Failed,
    Reversed
}

public enum WithdrawalType
{
    PartialRedemption,
    FullRedemption,
    Portability,
    Benefit
}
