namespace OpenFinance.Shared.Contracts;

/// <summary>
/// Defines the contract for credit operations data services provided to bank participants.
/// Aligned with the Open Finance Brasil Loans, Financings, and Unarranged Overdraft APIs (Phase 2).
/// </summary>
public interface ILoanContract
{
    // Loans
    Task<LoanListResponse> GetLoansAsync(string userId, Guid consentId, CancellationToken ct = default);
    Task<LoanDetailsResponse?> GetLoanDetailsAsync(Guid contractId, Guid consentId, CancellationToken ct = default);
    Task<LoanPaymentListResponse> GetLoanPaymentsAsync(Guid contractId, Guid consentId, CancellationToken ct = default);
    Task<LoanInstalmentListResponse> GetLoanInstalmentsAsync(Guid contractId, Guid consentId, CancellationToken ct = default);
    Task<LoanWarrantyListResponse> GetLoanWarrantiesAsync(Guid contractId, Guid consentId, CancellationToken ct = default);

    // Financings
    Task<FinancingListResponse> GetFinancingsAsync(string userId, Guid consentId, CancellationToken ct = default);
    Task<FinancingDetailsResponse?> GetFinancingDetailsAsync(Guid contractId, Guid consentId, CancellationToken ct = default);
    Task<LoanPaymentListResponse> GetFinancingPaymentsAsync(Guid contractId, Guid consentId, CancellationToken ct = default);
    Task<LoanInstalmentListResponse> GetFinancingInstalmentsAsync(Guid contractId, Guid consentId, CancellationToken ct = default);

    // Unarranged Overdraft
    Task<OverdraftListResponse> GetOverdraftsAsync(string userId, Guid consentId, CancellationToken ct = default);
    Task<OverdraftDetailsResponse?> GetOverdraftDetailsAsync(Guid contractId, Guid consentId, CancellationToken ct = default);
}

// ─── Loans ──────────────────────────────────────────────────────────────────

public record LoanListResponse(IReadOnlyList<LoanSummary> Loans);

public record LoanSummary(
    Guid ContractId,
    string ContractNumber,
    LoanType Type,
    string ProductName,
    ContractStatus Status,
    decimal ContractAmount,
    string Currency,
    DateOnly ContractDate,
    DateOnly DueDate
);

public record LoanDetailsResponse(
    Guid ContractId,
    string ContractNumber,
    LoanType Type,
    string ProductName,
    string CompanyCnpj,
    ContractStatus Status,
    decimal ContractAmount,
    decimal OutstandingBalance,
    decimal InterestRate,
    InterestRateType InterestRateType,
    RateIndexer Indexer,
    string Currency,
    DateOnly ContractDate,
    DateOnly DueDate,
    DateOnly SettlementDate,
    int InstalmentCount,
    int PaidInstalmentCount,
    decimal CET,
    AmortizationType AmortizationType,
    IReadOnlyList<ContractFee> Fees
);

public record LoanPaymentListResponse(IReadOnlyList<LoanPaymentSummary> Payments);

public record LoanPaymentSummary(
    Guid PaymentId,
    DateOnly PaymentDate,
    decimal PaidAmount,
    decimal PrincipalAmount,
    decimal InterestAmount,
    decimal FeesAmount,
    decimal ChargesAmount,
    string Currency,
    bool IsOverdue
);

public record LoanInstalmentListResponse(IReadOnlyList<LoanInstalmentSummary> Instalments);

public record LoanInstalmentSummary(
    int InstalmentNumber,
    DateOnly DueDate,
    decimal TotalAmount,
    decimal PrincipalAmount,
    decimal InterestAmount,
    decimal FeesAmount,
    string Currency,
    InstalmentStatus Status
);

public record LoanWarrantyListResponse(IReadOnlyList<LoanWarrantySummary> Warranties);

public record LoanWarrantySummary(
    Guid WarrantyId,
    WarrantyType Type,
    string WarrantySubType,
    string Currency,
    decimal Amount
);

public record ContractFee(
    string FeeName,
    string FeeCode,
    FeeChargeType ChargeType,
    decimal Amount,
    decimal Rate
);

// ─── Financings ─────────────────────────────────────────────────────────────

public record FinancingListResponse(IReadOnlyList<FinancingSummary> Financings);

public record FinancingSummary(
    Guid ContractId,
    string ContractNumber,
    FinancingType Type,
    string ProductName,
    ContractStatus Status,
    decimal ContractAmount,
    string Currency,
    DateOnly ContractDate,
    DateOnly DueDate
);

public record FinancingDetailsResponse(
    Guid ContractId,
    string ContractNumber,
    FinancingType Type,
    string ProductName,
    string CompanyCnpj,
    ContractStatus Status,
    decimal ContractAmount,
    decimal OutstandingBalance,
    decimal InterestRate,
    InterestRateType InterestRateType,
    RateIndexer Indexer,
    string Currency,
    DateOnly ContractDate,
    DateOnly DueDate,
    int InstalmentCount,
    int PaidInstalmentCount,
    decimal CET,
    AmortizationType AmortizationType
);

// ─── Unarranged Overdraft ───────────────────────────────────────────────────

public record OverdraftListResponse(IReadOnlyList<OverdraftSummary> Overdrafts);

public record OverdraftSummary(
    Guid ContractId,
    string ContractNumber,
    ContractStatus Status,
    decimal ContractAmount,
    string Currency,
    DateOnly ContractDate
);

public record OverdraftDetailsResponse(
    Guid ContractId,
    string ContractNumber,
    string CompanyCnpj,
    ContractStatus Status,
    decimal ContractAmount,
    decimal OutstandingBalance,
    decimal InterestRate,
    InterestRateType InterestRateType,
    RateIndexer Indexer,
    string Currency,
    DateOnly ContractDate,
    IReadOnlyList<ContractFee> Fees
);

// ─── Enumerations ───────────────────────────────────────────────────────────

public enum LoanType
{
    PersonalLoan,
    PersonalCreditWithGuarantee,
    VehicleLoan,
    PayrollLoan,
    MicrocreditLoan,
    Other
}

public enum FinancingType
{
    HomeFinancing,
    VehicleFinancing,
    RealEstateFinancing,
    InfrastructureFinancing,
    Other
}

public enum ContractStatus
{
    Active,
    Inactive,
    PendingDisbursement,
    Overdue,
    WrittenOff,
    Settled
}

public enum InterestRateType
{
    Simple,
    Compound
}

public enum AmortizationType
{
    SAC,
    Price,
    SAM,
    Other
}

public enum InstalmentStatus
{
    Pending,
    Paid,
    Overdue,
    PartiallyPaid
}

public enum WarrantyType
{
    Pledge,
    Mortgage,
    Surety,
    Lien,
    Fiduciary,
    Other
}

public enum FeeChargeType
{
    Minimum,
    Maximum,
    Fixed,
    Percentage
}
