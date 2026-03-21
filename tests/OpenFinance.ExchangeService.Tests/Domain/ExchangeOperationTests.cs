using FluentAssertions;
using OpenFinance.ExchangeService.Domain.Entities;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.ExchangeService.Tests.Domain;

public class ExchangeOperationTests
{
    private static ExchangeOperation ValidOperation() =>
        ExchangeOperation.Create(
            userId: "user-001",
            operationNumber: "EX-2024-001",
            operationType: ExchangeOperationType.Purchase,
            category: ExchangeCategory.Travel,
            foreignCurrency: "USD",
            localCurrency: "BRL",
            operationDate: new DateOnly(2024, 3, 10),
            deliveryDate: new DateOnly(2024, 3, 12),
            foreignCurrencyAmount: 1_000m,
            localCurrencyAmount: 5_100m,
            exchangeRate: 5.10m,
            vetAmount: 5_125m,
            iofAmount: 30m,
            irAmount: 0m,
            counterpartyName: "Banco Internacional",
            counterpartyCountry: "BR",
            deliveryType: ExchangeDeliveryType.Wire);

    [Fact]
    public void Create_WithValidData_ShouldCreateOpenOperation()
    {
        var operation = ValidOperation();

        operation.Status.Should().Be(ExchangeOperationStatus.Open);
        operation.ForeignCurrency.Should().Be("USD");
        operation.ExchangeRate.Should().Be(5.10m);
    }

    [Fact]
    public void Close_WhenOpen_ShouldSetClosedStatus()
    {
        var operation = ValidOperation();
        operation.Close();

        operation.Status.Should().Be(ExchangeOperationStatus.Closed);
    }

    [Fact]
    public void Cancel_WhenOpen_ShouldSetCancelledStatus()
    {
        var operation = ValidOperation();
        operation.Cancel();

        operation.Status.Should().Be(ExchangeOperationStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenClosed_ShouldThrow()
    {
        var operation = ValidOperation();
        operation.Close();

        var act = () => operation.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Create_WithDeliveryDateBeforeOperationDate_ShouldThrow()
    {
        var act = () => ExchangeOperation.Create(
            "user-001", "EX-001", ExchangeOperationType.Purchase, ExchangeCategory.Travel,
            "USD", "BRL",
            operationDate: new DateOnly(2024, 3, 15),
            deliveryDate: new DateOnly(2024, 3, 10),
            1_000m, 5_100m, 5.10m, 5_125m, 30m, 0m,
            "Counterparty", "BR", ExchangeDeliveryType.Cash);

        act.Should().Throw<ArgumentException>().WithMessage("*Delivery*");
    }

    [Fact]
    public void Create_WithZeroExchangeRate_ShouldThrow()
    {
        var act = () => ExchangeOperation.Create(
            "user-001", "EX-001", ExchangeOperationType.Purchase, ExchangeCategory.Travel,
            "USD", "BRL",
            new DateOnly(2024, 3, 10), new DateOnly(2024, 3, 12),
            1_000m, 5_100m, 0m, 5_125m, 30m, 0m,
            "Counterparty", "BR", ExchangeDeliveryType.Cash);

        act.Should().Throw<ArgumentException>().WithMessage("*rate*");
    }
}
