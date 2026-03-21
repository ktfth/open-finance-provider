using FluentAssertions;
using Moq;
using OpenFinance.ExchangeService.Application.UseCases;
using OpenFinance.ExchangeService.Domain.Entities;
using OpenFinance.ExchangeService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.ExchangeService.Tests.Application;

public class GetExchangeEventsUseCaseTests
{
    private readonly Mock<IExchangeRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetExchangeEventsUseCase _sut;

    public GetExchangeEventsUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetExchangeEventsUseCase(_repository.Object, _consentValidator.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOperationHasEvents_ShouldReturnMappedSummaries()
    {
        var operationId = Guid.NewGuid();
        var evt = ExchangeEvent.Create(
            operationId,
            ExchangeEventType.Settlement,
            new DateOnly(2024, 3, 12),
            foreignCurrencyAmount: 1_000m,
            localCurrencyAmount: 5_100m,
            foreignCurrency: "USD",
            localCurrency: "BRL",
            exchangeRate: 5.10m);

        _repository.Setup(r => r.GetEventsAsync(operationId, default))
            .ReturnsAsync(new[] { evt });

        var result = await _sut.ExecuteAsync(operationId, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Events.Should().HaveCount(1);
        result.Value.Events[0].EventType.Should().Be(ExchangeEventType.Settlement);
        result.Value.Events[0].ForeignCurrencyAmount.Should().Be(1_000m);
        result.Value.Events[0].ExchangeRate.Should().Be(5.10m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOperationHasNoEvents_ShouldReturnEmptyList()
    {
        var operationId = Guid.NewGuid();
        _repository.Setup(r => r.GetEventsAsync(operationId, default))
            .ReturnsAsync(Array.Empty<ExchangeEvent>());

        var result = await _sut.ExecuteAsync(operationId, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Events.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenOperationHasMultipleEvents_ShouldReturnAllEvents()
    {
        var operationId = Guid.NewGuid();
        var closingEvent = ExchangeEvent.Create(
            operationId, ExchangeEventType.Closing, new DateOnly(2024, 3, 11),
            1_000m, 5_100m, "USD", "BRL", 5.10m);
        var settlementEvent = ExchangeEvent.Create(
            operationId, ExchangeEventType.Settlement, new DateOnly(2024, 3, 12),
            1_000m, 5_100m, "USD", "BRL", 5.10m);

        _repository.Setup(r => r.GetEventsAsync(operationId, default))
            .ReturnsAsync(new[] { closingEvent, settlementEvent });

        var result = await _sut.ExecuteAsync(operationId, Guid.NewGuid());

        result.Value.Events.Should().HaveCount(2);
        result.Value.Events.Should().Contain(e => e.EventType == ExchangeEventType.Closing);
        result.Value.Events.Should().Contain(e => e.EventType == ExchangeEventType.Settlement);
    }
}
