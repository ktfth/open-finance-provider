using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.ExchangeService.Domain.Entities;

/// <summary>
/// Represents a foreign exchange operation (câmbio) executed by a bank client.
/// Aligned with the Open Finance Brasil Exchange API specification (Phase 4).
/// </summary>
public class ExchangeOperation : Entity
{
    private ExchangeOperation() { }

    public static ExchangeOperation Create(
        string userId,
        string operationNumber,
        ExchangeOperationType operationType,
        ExchangeCategory category,
        string foreignCurrency,
        string localCurrency,
        DateOnly operationDate,
        DateOnly deliveryDate,
        decimal foreignCurrencyAmount,
        decimal localCurrencyAmount,
        decimal exchangeRate,
        decimal vetAmount,
        decimal iofAmount,
        decimal irAmount,
        string counterpartyName,
        string counterpartyCountry,
        ExchangeDeliveryType deliveryType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(foreignCurrency);
        ArgumentException.ThrowIfNullOrWhiteSpace(localCurrency);
        ArgumentException.ThrowIfNullOrWhiteSpace(counterpartyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(counterpartyCountry);

        if (foreignCurrencyAmount <= 0)
            throw new ArgumentException("Foreign currency amount must be positive.", nameof(foreignCurrencyAmount));
        if (exchangeRate <= 0)
            throw new ArgumentException("Exchange rate must be positive.", nameof(exchangeRate));
        if (deliveryDate < operationDate)
            throw new ArgumentException("Delivery date must not be before operation date.", nameof(deliveryDate));

        return new ExchangeOperation
        {
            UserId = userId,
            OperationNumber = operationNumber,
            OperationType = operationType,
            Category = category,
            Status = ExchangeOperationStatus.Open,
            ForeignCurrency = foreignCurrency,
            LocalCurrency = localCurrency,
            OperationDate = operationDate,
            DeliveryDate = deliveryDate,
            ForeignCurrencyAmount = foreignCurrencyAmount,
            LocalCurrencyAmount = localCurrencyAmount,
            ExchangeRate = exchangeRate,
            VETAmount = vetAmount,
            IOFAmount = iofAmount,
            IRAmount = irAmount,
            CounterpartyName = counterpartyName,
            CounterpartyCountry = counterpartyCountry,
            DeliveryType = deliveryType
        };
    }

    public string UserId { get; private set; } = default!;
    public string OperationNumber { get; private set; } = default!;
    public ExchangeOperationType OperationType { get; private set; }
    public ExchangeCategory Category { get; private set; }
    public ExchangeOperationStatus Status { get; private set; }
    public string ForeignCurrency { get; private set; } = default!;
    public string LocalCurrency { get; private set; } = default!;
    public DateOnly OperationDate { get; private set; }
    public DateOnly DeliveryDate { get; private set; }
    public decimal ForeignCurrencyAmount { get; private set; }
    public decimal LocalCurrencyAmount { get; private set; }
    public decimal ExchangeRate { get; private set; }
    public decimal VETAmount { get; private set; }
    public decimal IOFAmount { get; private set; }
    public decimal IRAmount { get; private set; }
    public string CounterpartyName { get; private set; } = default!;
    public string CounterpartyCountry { get; private set; } = default!;
    public ExchangeDeliveryType DeliveryType { get; private set; }

    /// <summary>Closes the operation after successful settlement.</summary>
    public void Close()
    {
        if (Status == ExchangeOperationStatus.Cancelled)
            throw new InvalidOperationException("Cannot close a cancelled operation.");

        Status = ExchangeOperationStatus.Closed;
        SetUpdated();
    }

    /// <summary>Cancels the operation before settlement.</summary>
    public void Cancel()
    {
        if (Status == ExchangeOperationStatus.Closed)
            throw new InvalidOperationException("Cannot cancel a closed operation.");

        Status = ExchangeOperationStatus.Cancelled;
        SetUpdated();
    }
}
