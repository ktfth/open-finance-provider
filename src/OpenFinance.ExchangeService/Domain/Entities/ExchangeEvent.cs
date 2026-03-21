using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.ExchangeService.Domain.Entities;

/// <summary>
/// Represents a lifecycle event associated with a foreign exchange operation.
/// Events track closings, settlements, partial settlements, cancellations and amendments.
/// </summary>
public class ExchangeEvent : Entity
{
    private ExchangeEvent() { }

    public static ExchangeEvent Create(
        Guid operationId,
        ExchangeEventType eventType,
        DateOnly eventDate,
        decimal foreignCurrencyAmount,
        decimal localCurrencyAmount,
        string foreignCurrency,
        string localCurrency,
        decimal exchangeRate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(foreignCurrency);
        ArgumentException.ThrowIfNullOrWhiteSpace(localCurrency);

        if (operationId == Guid.Empty)
            throw new ArgumentException("OperationId must not be empty.", nameof(operationId));
        if (foreignCurrencyAmount < 0)
            throw new ArgumentException("Foreign currency amount must not be negative.", nameof(foreignCurrencyAmount));
        if (exchangeRate <= 0)
            throw new ArgumentException("Exchange rate must be positive.", nameof(exchangeRate));

        return new ExchangeEvent
        {
            OperationId = operationId,
            EventType = eventType,
            EventDate = eventDate,
            ForeignCurrencyAmount = foreignCurrencyAmount,
            LocalCurrencyAmount = localCurrencyAmount,
            ForeignCurrency = foreignCurrency,
            LocalCurrency = localCurrency,
            ExchangeRate = exchangeRate
        };
    }

    public Guid OperationId { get; private set; }
    public ExchangeEventType EventType { get; private set; }
    public DateOnly EventDate { get; private set; }
    public decimal ForeignCurrencyAmount { get; private set; }
    public decimal LocalCurrencyAmount { get; private set; }
    public string ForeignCurrency { get; private set; } = default!;
    public string LocalCurrency { get; private set; } = default!;
    public decimal ExchangeRate { get; private set; }
}
