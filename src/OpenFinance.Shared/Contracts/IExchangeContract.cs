namespace OpenFinance.Shared.Contracts;

/// <summary>
/// Defines the contract for foreign exchange (câmbio) data services provided to bank participants.
/// Aligned with the Open Finance Brasil Exchange API specification (Phase 4).
/// </summary>
public interface IExchangeContract
{
    Task<ExchangeOperationListResponse> GetExchangeOperationsAsync(string userId, Guid consentId, CancellationToken ct = default);
    Task<ExchangeOperationDetailsResponse?> GetExchangeOperationDetailsAsync(Guid operationId, Guid consentId, CancellationToken ct = default);
    Task<ExchangeEventListResponse> GetExchangeEventsAsync(Guid operationId, Guid consentId, CancellationToken ct = default);
}

public record ExchangeOperationListResponse(IReadOnlyList<ExchangeOperationSummary> Operations);

public record ExchangeOperationSummary(
    Guid OperationId,
    string OperationNumber,
    ExchangeOperationType OperationType,
    ExchangeCategory Category,
    ExchangeOperationStatus Status,
    string ForeignCurrency,
    string LocalCurrency,
    DateOnly OperationDate,
    decimal ForeignCurrencyAmount,
    decimal LocalCurrencyAmount
);

public record ExchangeOperationDetailsResponse(
    Guid OperationId,
    string OperationNumber,
    ExchangeOperationType OperationType,
    ExchangeCategory Category,
    ExchangeOperationStatus Status,
    string ForeignCurrency,
    string LocalCurrency,
    DateOnly OperationDate,
    DateOnly DeliveryDate,
    decimal ForeignCurrencyAmount,
    decimal LocalCurrencyAmount,
    decimal ExchangeRate,
    decimal VETAmount,
    decimal IOFAmount,
    decimal IRAmount,
    string CounterpartyName,
    string CounterpartyCountry,
    ExchangeDeliveryType DeliveryType
);

public record ExchangeEventListResponse(IReadOnlyList<ExchangeEventSummary> Events);

public record ExchangeEventSummary(
    Guid EventId,
    ExchangeEventType EventType,
    DateOnly EventDate,
    decimal ForeignCurrencyAmount,
    decimal LocalCurrencyAmount,
    string ForeignCurrency,
    string LocalCurrency,
    decimal ExchangeRate
);

// ─── Enumerations ────────────────────────────────────────────────────────────

public enum ExchangeOperationType
{
    Purchase,
    Sale
}

public enum ExchangeCategory
{
    CommercialExchange,
    FinancialExchange,
    Travel,
    InternationalTransfer,
    Import,
    Export,
    Other
}

public enum ExchangeOperationStatus
{
    Open,
    Closed,
    Cancelled,
    PendingSettlement
}

public enum ExchangeDeliveryType
{
    Cash,
    Wire,
    PrepaidCard,
    TravelersCheck,
    Other
}

public enum ExchangeEventType
{
    Closing,
    Settlement,
    PartialSettlement,
    Cancellation,
    Amendment
}
