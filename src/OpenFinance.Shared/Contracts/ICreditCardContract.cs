namespace OpenFinance.Shared.Contracts;

/// <summary>
/// Defines the contract for credit card data services provided to bank participants.
/// Aligned with the Open Finance Brasil Credit Cards API specification.
/// </summary>
public interface ICreditCardContract
{
    Task<CardAccountListResponse> GetCardAccountsAsync(string userId, Guid consentId, CancellationToken ct = default);
    Task<CardAccountDetailsResponse?> GetCardAccountDetailsAsync(Guid cardAccountId, Guid consentId, CancellationToken ct = default);
    Task<CardLimitsResponse?> GetCardLimitsAsync(Guid cardAccountId, Guid consentId, CancellationToken ct = default);
    Task<CardBillListResponse> GetCardBillsAsync(Guid cardAccountId, Guid consentId, CancellationToken ct = default);
    Task<CardBillTransactionListResponse> GetCardBillTransactionsAsync(Guid cardAccountId, Guid billId, Guid consentId, CancellationToken ct = default);
}

public record CardAccountListResponse(IReadOnlyList<CardAccountSummary> CardAccounts);

public record CardAccountSummary(
    Guid CardAccountId,
    string LastFourDigits,
    CardBrand Brand,
    CardType CardType,
    string HolderName,
    CardAccountStatus Status
);

public record CardAccountDetailsResponse(
    Guid CardAccountId,
    string LastFourDigits,
    CardBrand Brand,
    CardType CardType,
    CardNetworkType NetworkType,
    string HolderName,
    string HolderCpf,
    CardAccountStatus Status,
    DateTime DueDate,
    int PaymentDay
);

public record CardLimitsResponse(
    Guid CardAccountId,
    IReadOnlyList<CreditLimit> Limits
);

public record CreditLimit(
    CreditLimitType LimitType,
    string CreditLineLimitType,
    string ConsolidationType,
    string IdentificationNumber,
    string LineName,
    string LineNameAdditionalInfo,
    bool IsLimitFlexible,
    decimal LimitAmountTotal,
    string LimitTransactionCurrency,
    decimal UsedAmountTotal,
    string UsedTransactionCurrency,
    decimal AvailableAmountTotal,
    string AvailableTransactionCurrency
);

public record CardBillListResponse(IReadOnlyList<CardBillSummary> Bills);

public record CardBillSummary(
    Guid BillId,
    DateOnly DueDate,
    decimal TotalAmount,
    string TotalAmountCurrency,
    decimal MinimumPaymentAmount,
    string MinimumPaymentAmountCurrency,
    bool IsInstalment,
    CardBillStatus Status
);

public record CardBillTransactionListResponse(IReadOnlyList<CardBillTransactionSummary> Transactions);

public record CardBillTransactionSummary(
    Guid TransactionId,
    string IdentificationNumber,
    string LineName,
    string TransactionName,
    string BillIdentification,
    CardTransactionType TransactionType,
    decimal Amount,
    string Currency,
    DateTime TransactionDateTime,
    decimal BillPostDate,
    decimal PayeeMCC
);

public enum CardBrand
{
    Visa,
    Mastercard,
    AmericanExpress,
    DinersClub,
    Hipercard,
    Elo,
    Other
}

public enum CardType
{
    Credit,
    Debit,
    Multiple
}

public enum CardNetworkType
{
    Visa,
    Mastercard,
    AmericanExpress,
    DinersClub,
    Hipercard,
    Elo,
    Other
}

public enum CardAccountStatus
{
    Active,
    Blocked,
    Cancelled
}

public enum CardBillStatus
{
    Open,
    Closed,
    Overdue,
    Paid
}

public enum CardTransactionType
{
    Purchase,
    Withdrawal,
    Instalment,
    Fee,
    Interest,
    Refund,
    Payment,
    Other
}

public enum CreditLimitType
{
    Total,
    Individual
}
