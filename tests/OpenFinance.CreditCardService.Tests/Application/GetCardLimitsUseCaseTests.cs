using FluentAssertions;
using Moq;
using OpenFinance.CreditCardService.Application.UseCases;
using OpenFinance.CreditCardService.Domain.Entities;
using OpenFinance.CreditCardService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.CreditCardService.Tests.Application;

public class GetCardLimitsUseCaseTests
{
    private readonly Mock<ICardRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetCardLimitsUseCase _sut;

    public GetCardLimitsUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetCardLimitsUseCase(_repository.Object, _consentValidator.Object);
    }

    private static CardAccount MakeCard() =>
        CardAccount.Create("user-1", "4321", CardBrand.Visa, CardType.Credit,
            CardNetworkType.Visa, "João Silva", "123.456.789-00", paymentDay: 10);

    [Fact]
    public async Task ExecuteAsync_WithLimits_ShouldReturnLimitsWithCorrectAvailableAmount()
    {
        var card = MakeCard();
        var limit = CardLimit.Create(card.Id, CreditLimitType.Total, "LIMITE_TOTAL",
            "CONSOLIDADO", "LMT-001", "Limite Total", false, 10000m, "BRL");
        limit.UpdateUsage(3000m);

        _repository.Setup(r => r.GetCardAccountByIdAsync(card.Id, default)).ReturnsAsync(card);
        _repository.Setup(r => r.GetCardLimitsAsync(card.Id, default))
            .ReturnsAsync(new List<CardLimit> { limit });

        var result = await _sut.ExecuteAsync(card.Id, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Limits.Should().HaveCount(1);
        var l = result.Value.Limits[0];
        l.LimitAmountTotal.Should().Be(10000m);
        l.UsedAmountTotal.Should().Be(3000m);
        l.AvailableAmountTotal.Should().Be(7000m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCardNotFound_ShouldReturnNullValue()
    {
        _repository.Setup(r => r.GetCardAccountByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((CardAccount?)null);

        var result = await _sut.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenConsentServiceUnavailable_ShouldReturnFailure()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.ServiceUnavailable("CONSENT_SERVICE_UNAVAILABLE: timeout"));

        var result = await _sut.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("CONSENT_SERVICE_UNAVAILABLE");
    }
}
