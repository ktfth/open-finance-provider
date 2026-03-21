namespace OpenFinance.Shared.Contracts;

/// <summary>
/// Defines the contract for insurance data services provided to bank participants.
/// Aligned with the Open Finance Brasil Insurance API specification (Phase 4).
/// </summary>
public interface IInsuranceContract
{
    Task<InsuranceListResponse> GetInsurancesAsync(string userId, Guid consentId, CancellationToken ct = default);
    Task<InsuranceDetailsResponse?> GetInsuranceDetailsAsync(Guid insuranceId, Guid consentId, CancellationToken ct = default);
    Task<InsurancePremiumResponse?> GetInsurancePremiumAsync(Guid insuranceId, Guid consentId, CancellationToken ct = default);
    Task<InsuranceClaimListResponse> GetInsuranceClaimsAsync(Guid insuranceId, Guid consentId, CancellationToken ct = default);
    Task<InsuranceCoverageListResponse> GetInsuranceCoveragesAsync(Guid insuranceId, Guid consentId, CancellationToken ct = default);
}

public record InsuranceListResponse(IReadOnlyList<InsuranceSummary> Insurances);

public record InsuranceSummary(
    Guid InsuranceId,
    InsuranceType Type,
    string ProductName,
    string InsurerName,
    string InsurerCnpj,
    InsuranceStatus Status,
    string PolicyNumber,
    string Currency
);

public record InsuranceDetailsResponse(
    Guid InsuranceId,
    InsuranceType Type,
    string ProductName,
    string InsurerName,
    string InsurerCnpj,
    InsuranceStatus Status,
    string PolicyNumber,
    DateOnly ProposalDate,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    decimal InsuredAmount,
    decimal PremiumAmount,
    string Currency,
    int GracePeriodDays,
    string InsuredCpfCnpj,
    string InsuredName,
    string BeneficiaryName
);

public record InsurancePremiumResponse(
    Guid InsuranceId,
    decimal TotalPremium,
    decimal PaidAmount,
    decimal OutstandingAmount,
    string Currency,
    PaymentFrequency PaymentFrequency,
    DateOnly NextDueDate,
    IReadOnlyList<InsurancePremiumPayment> Payments
);

public record InsurancePremiumPayment(
    Guid PaymentId,
    DateOnly DueDate,
    decimal Amount,
    string Currency,
    PremiumPaymentStatus Status
);

public record InsuranceClaimListResponse(IReadOnlyList<InsuranceClaimSummary> Claims);

public record InsuranceClaimSummary(
    Guid ClaimId,
    string ClaimNumber,
    DateOnly OccurrenceDate,
    DateOnly NotificationDate,
    decimal ClaimedAmount,
    decimal? ApprovedAmount,
    string Currency,
    ClaimStatus Status
);

public record InsuranceCoverageListResponse(IReadOnlyList<InsuranceCoverageSummary> Coverages);

public record InsuranceCoverageSummary(
    Guid CoverageId,
    string CoverageName,
    CoverageType Type,
    decimal InsuredAmount,
    decimal DeductibleAmount,
    string Currency,
    bool IsMainCoverage
);

// ─── Enumerations ────────────────────────────────────────────────────────────

public enum InsuranceType
{
    Life,
    Home,
    Auto,
    Health,
    Travel,
    PersonalAccident,
    CivilLiability,
    BusinessProperty,
    Crop,
    Other
}

public enum InsuranceStatus
{
    Active,
    Suspended,
    Cancelled,
    Expired,
    PendingActivation
}

public enum PaymentFrequency
{
    Single,
    Monthly,
    Quarterly,
    Semiannual,
    Annual
}

public enum PremiumPaymentStatus
{
    Pending,
    Paid,
    Overdue,
    Cancelled
}

public enum ClaimStatus
{
    Open,
    UnderAnalysis,
    Approved,
    Denied,
    PartiallyApproved,
    Paid,
    Closed
}

public enum CoverageType
{
    Death,
    Disability,
    PropertyDamage,
    Theft,
    Fire,
    NaturalDisaster,
    ThirdPartyLiability,
    Medical,
    Hospitalization,
    Other
}
