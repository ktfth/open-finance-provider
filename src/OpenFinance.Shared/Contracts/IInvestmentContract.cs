namespace OpenFinance.Shared.Contracts;

/// <summary>
/// Defines the contract for investment data services provided to bank participants.
/// Aligned with the Open Finance Brasil Investments API specification.
/// Covers Fixed Income (Renda Fixa), Variable Income (Renda Variável), and Treasury Bonds (Tesouro Direto).
/// </summary>
public interface IInvestmentContract
{
    // Fixed Income
    Task<FixedIncomeListResponse> GetFixedIncomeAsync(string userId, Guid consentId, CancellationToken ct = default);
    Task<FixedIncomeDetailsResponse?> GetFixedIncomeDetailsAsync(Guid investmentId, Guid consentId, CancellationToken ct = default);
    Task<FixedIncomeBalanceResponse?> GetFixedIncomeBalanceAsync(Guid investmentId, Guid consentId, CancellationToken ct = default);
    Task<FixedIncomeTransactionListResponse> GetFixedIncomeTransactionsAsync(Guid investmentId, Guid consentId, DateOnly from, DateOnly to, CancellationToken ct = default);

    // Variable Income
    Task<VariableIncomeListResponse> GetVariableIncomeAsync(string userId, Guid consentId, CancellationToken ct = default);
    Task<VariableIncomeDetailsResponse?> GetVariableIncomeDetailsAsync(Guid investmentId, Guid consentId, CancellationToken ct = default);
    Task<VariableIncomeBalanceResponse?> GetVariableIncomeBalanceAsync(Guid investmentId, Guid consentId, CancellationToken ct = default);
    Task<VariableIncomeTransactionListResponse> GetVariableIncomeTransactionsAsync(Guid investmentId, Guid consentId, DateOnly from, DateOnly to, CancellationToken ct = default);

    // Treasury Bonds (Tesouro Direto)
    Task<TreasuryBondListResponse> GetTreasuryBondsAsync(string userId, Guid consentId, CancellationToken ct = default);
    Task<TreasuryBondDetailsResponse?> GetTreasuryBondDetailsAsync(Guid investmentId, Guid consentId, CancellationToken ct = default);
    Task<TreasuryBondBalanceResponse?> GetTreasuryBondBalanceAsync(Guid investmentId, Guid consentId, CancellationToken ct = default);
}

// ─── Fixed Income ───────────────────────────────────────────────────────────

public record FixedIncomeListResponse(IReadOnlyList<FixedIncomeSummary> Investments);

public record FixedIncomeSummary(
    Guid InvestmentId,
    FixedIncomeType Type,
    string ProductName,
    string Issuer,
    DateOnly MaturityDate,
    decimal GrossAmount,
    string Currency
);

public record FixedIncomeDetailsResponse(
    Guid InvestmentId,
    FixedIncomeType Type,
    string ProductName,
    string Issuer,
    string ISIN,
    DateOnly IssueDate,
    DateOnly MaturityDate,
    decimal FaceValue,
    decimal PurchaseUnitPrice,
    decimal Quantity,
    decimal GrossAmount,
    decimal NetAmount,
    string Currency,
    RateIndexer Indexer,
    decimal IndexerPercentage,
    decimal PreFixedRate,
    decimal PostFixedRate,
    decimal TaxExemptionPercentage,
    RemunType RemunerationType
);

public record FixedIncomeBalanceResponse(
    Guid InvestmentId,
    DateOnly ReferenceDate,
    decimal GrossAmount,
    decimal NetAmount,
    decimal IncomeTax,
    decimal IOFTax,
    decimal PurchaseUnitPrice,
    decimal UpdatedUnitPrice,
    decimal Quantity,
    decimal Yield,
    string Currency
);

public record FixedIncomeTransactionListResponse(IReadOnlyList<FixedIncomeTransactionSummary> Transactions);

public record FixedIncomeTransactionSummary(
    Guid TransactionId,
    FixedIncomeTransactionType Type,
    DateOnly TransactionDate,
    decimal Quantity,
    decimal UnitPrice,
    decimal GrossValue,
    decimal TaxValue,
    decimal NetValue,
    string Currency
);

// ─── Variable Income ─────────────────────────────────────────────────────────

public record VariableIncomeListResponse(IReadOnlyList<VariableIncomeSummary> Investments);

public record VariableIncomeSummary(
    Guid InvestmentId,
    VariableIncomeType Type,
    string Ticker,
    string ProductName,
    decimal Quantity,
    decimal GrossAmount,
    string Currency
);

public record VariableIncomeDetailsResponse(
    Guid InvestmentId,
    VariableIncomeType Type,
    string Ticker,
    string ProductName,
    string ISIN,
    decimal Quantity,
    decimal AveragePrice,
    decimal CurrentPrice,
    decimal GrossAmount,
    decimal NetAmount,
    decimal IncomeTax,
    string Currency,
    DateOnly LastQuoteDate
);

public record VariableIncomeBalanceResponse(
    Guid InvestmentId,
    DateOnly ReferenceDate,
    decimal Quantity,
    decimal UnitPrice,
    decimal GrossAmount,
    decimal NetAmount,
    decimal IncomeTax,
    string Currency,
    decimal Yield,
    decimal YieldPercentage
);

public record VariableIncomeTransactionListResponse(IReadOnlyList<VariableIncomeTransactionSummary> Transactions);

public record VariableIncomeTransactionSummary(
    Guid TransactionId,
    VariableIncomeTransactionType Type,
    DateOnly TransactionDate,
    decimal Quantity,
    decimal UnitPrice,
    decimal GrossValue,
    decimal BrokerageFee,
    decimal TaxValue,
    decimal NetValue,
    string Currency
);

// ─── Treasury Bonds (Tesouro Direto) ──────────────────────────────────────

public record TreasuryBondListResponse(IReadOnlyList<TreasuryBondSummary> Bonds);

public record TreasuryBondSummary(
    Guid InvestmentId,
    string ProductName,
    TreasuryBondType BondType,
    DateOnly MaturityDate,
    decimal InvestedAmount,
    decimal CurrentAmount,
    string Currency
);

public record TreasuryBondDetailsResponse(
    Guid InvestmentId,
    string ProductName,
    TreasuryBondType BondType,
    DateOnly PurchaseDate,
    DateOnly MaturityDate,
    decimal NominalQuantity,
    decimal NominalUnitPrice,
    decimal UpdatedUnitPrice,
    decimal GrossAmount,
    decimal IncomeTax,
    decimal IOFTax,
    decimal NetAmount,
    string Currency,
    decimal RateType,
    decimal PurchaseIndexValue
);

public record TreasuryBondBalanceResponse(
    Guid InvestmentId,
    DateOnly ReferenceDate,
    decimal GrossAmount,
    decimal NetAmount,
    decimal IncomeTax,
    decimal IOFTax,
    decimal Yield,
    decimal UpdatedUnitPrice,
    string Currency
);

// ─── Enumerations ────────────────────────────────────────────────────────────

public enum FixedIncomeType
{
    /// <summary>Certificado de Depósito Bancário</summary>
    CDB,
    /// <summary>Letra de Crédito Imobiliário</summary>
    LCI,
    /// <summary>Letra de Crédito do Agronegócio</summary>
    LCA,
    /// <summary>Certificado de Recebíveis Imobiliários</summary>
    CRI,
    /// <summary>Certificado de Recebíveis do Agronegócio</summary>
    CRA,
    /// <summary>Debêntures</summary>
    Debenture,
    /// <summary>Letra Financeira</summary>
    LF,
    /// <summary>Fundo de Renda Fixa</summary>
    FixedIncomeFund,
    Other
}

public enum VariableIncomeType
{
    /// <summary>Ações (Stocks)</summary>
    Stock,
    /// <summary>Fundos de Investimento em Ações</summary>
    EquityFund,
    /// <summary>Brazilian Depositary Receipt</summary>
    BDR,
    /// <summary>Exchange-Traded Fund</summary>
    ETF,
    /// <summary>Fundo de Investimento Imobiliário</summary>
    FII,
    Other
}

public enum TreasuryBondType
{
    /// <summary>Tesouro Prefixado</summary>
    Prefixado,
    /// <summary>Tesouro IPCA+</summary>
    IPCAMais,
    /// <summary>Tesouro IPCA+ com juros semestrais</summary>
    IPCAMaisJurosSemestrais,
    /// <summary>Tesouro Selic</summary>
    Selic,
    /// <summary>Tesouro Prefixado com juros semestrais</summary>
    PrefixadoJurosSemestrais
}

public enum RateIndexer
{
    CDI,
    SELIC,
    IPCA,
    IGPM,
    TR,
    TJLP,
    PreFixed,
    Other
}

public enum RemunType
{
    PreFixed,
    PostFixed,
    Hybrid
}

public enum FixedIncomeTransactionType
{
    Purchase,
    Redemption,
    InterestPayment,
    Maturity,
    Transfer
}

public enum VariableIncomeTransactionType
{
    Buy,
    Sell,
    DividendPayment,
    InterestOnCapital,
    Subscription,
    Transfer,
    Split,
    Reverse
}
