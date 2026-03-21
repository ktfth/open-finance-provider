namespace OpenFinance.Shared.Contracts;

/// <summary>
/// Defines the contract for capitalization bond (título de capitalização) data services.
/// Aligned with the Open Finance Brasil Capitalization API specification (Phase 4).
/// </summary>
public interface ICapitalizationContract
{
    Task<CapitalizationBondListResponse> GetCapitalizationBondsAsync(string userId, Guid consentId, CancellationToken ct = default);
    Task<CapitalizationBondDetailsResponse?> GetCapitalizationBondDetailsAsync(Guid bondId, Guid consentId, CancellationToken ct = default);
    Task<CapitalizationBondPaymentListResponse> GetCapitalizationBondPaymentsAsync(Guid bondId, Guid consentId, CancellationToken ct = default);
}

public record CapitalizationBondListResponse(IReadOnlyList<CapitalizationBondSummary> Bonds);

public record CapitalizationBondSummary(
    Guid BondId,
    string BondNumber,
    CapitalizationModality Modality,
    string ProductName,
    string CompanyName,
    string CompanyCnpj,
    CapitalizationBondStatus Status,
    string Currency
);

public record CapitalizationBondDetailsResponse(
    Guid BondId,
    string BondNumber,
    CapitalizationModality Modality,
    string ProductName,
    string CompanyName,
    string CompanyCnpj,
    CapitalizationBondStatus Status,
    DateOnly ContractDate,
    DateOnly MaturityDate,
    int PaymentCount,
    decimal PaymentAmount,
    PaymentFrequency PaymentFrequency,
    decimal LatePaymentFine,
    decimal LatePaymentInterest,
    decimal RedemptionPercentage,
    decimal CurrentRedemptionValue,
    decimal PrizeDrawAmount,
    string Currency,
    decimal TotalPaidAmount,
    decimal MathematicalReserve,
    decimal SurrenderQuota
);

public record CapitalizationBondPaymentListResponse(IReadOnlyList<CapitalizationBondPaymentSummary> Payments);

public record CapitalizationBondPaymentSummary(
    Guid PaymentId,
    DateOnly DueDate,
    decimal Amount,
    string Currency,
    CapitalizationPaymentStatus Status
);

// ─── Enumerations ────────────────────────────────────────────────────────────

public enum CapitalizationModality
{
    Traditional,
    Incentive,
    Popular,
    CompulsoryPurchase
}

public enum CapitalizationBondStatus
{
    Active,
    Suspended,
    Redeemed,
    Expired
}

public enum CapitalizationPaymentStatus
{
    Pending,
    Paid,
    Overdue,
    Cancelled
}
