using FluentAssertions;
using Moq;
using OpenFinance.ExchangeService.Application.UseCases;
using OpenFinance.ExchangeService.Domain.Entities;
using OpenFinance.ExchangeService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.ExchangeService.Tests.Application;

public class GetExchangeOperationsUseCaseTests
{
    private readonly Mock<IExchangeRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetExchangeOperationsUseCase _sut;

    public GetExchangeOperationsUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetExchangeOperationsUseCase(_repository.Object, _consentValidator.Object);
    }

    private static ExchangeOperation ValidExchangeOperation() =>
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
            counterpartyName: "Banco Internacional S/A",
            counterpartyCountry: "BR",
            deliveryType: ExchangeDeliveryType.Wire);

    [Fact]
    public async Task ExecuteAsync_WhenUserHasOperations_ShouldReturnMappedSummaries()
    {
        var operation = ValidExchangeOperation();
        _repository.Setup(r => r.GetByUserIdAsync("user-001", default))
            .ReturnsAsync(new[] { operation });

        var result = await _sut.ExecuteAsync("user-001", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Operations.Should().HaveCount(1);
        result.Value.Operations[0].OperationNumber.Should().Be("EX-2024-001");
        result.Value.Operations[0].OperationType.Should().Be(ExchangeOperationType.Purchase);
        result.Value.Operations[0].ForeignCurrency.Should().Be("USD");
        result.Value.Operations[0].ForeignCurrencyAmount.Should().Be(1_000m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserHasNoOperations_ShouldReturnEmptyList()
    {
        _repository.Setup(r => r.GetByUserIdAsync("user-002", default))
            .ReturnsAsync(Array.Empty<ExchangeOperation>());

        var result = await _sut.ExecuteAsync("user-002", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Operations.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserHasMultipleOperations_ShouldReturnAll()
    {
        var op1 = ValidExchangeOperation();
        var op2 = ExchangeOperation.Create(
            "user-001", "EX-2024-002", ExchangeOperationType.Sale,
            ExchangeCategory.CommercialExchange, "EUR", "BRL",
            new DateOnly(2024, 4, 5), new DateOnly(2024, 4, 7),
            500m, 2_700m, 5.40m, 2_710m, 15m, 0m,
            "Banco ABC", "BR", ExchangeDeliveryType.Cash);

        _repository.Setup(r => r.GetByUserIdAsync("user-001", default))
            .ReturnsAsync(new[] { op1, op2 });

        var result = await _sut.ExecuteAsync("user-001", Guid.NewGuid());

        result.Value.Operations.Should().HaveCount(2);
        result.Value.Operations.Should().Contain(o => o.OperationType == ExchangeOperationType.Sale);
    }
}
