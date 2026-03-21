using FluentAssertions;
using Moq;
using OpenFinance.CreditCardService.Application.UseCases;
using OpenFinance.CreditCardService.Domain.Entities;
using OpenFinance.CreditCardService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.CreditCardService.Tests.Application;

public class GetCardAccountsUseCaseTests
{
    private readonly Mock<ICardRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetCardAccountsUseCase _sut;

    public GetCardAccountsUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetCardAccountsUseCase(_repository.Object, _consentValidator.Object);
    }

    private static CardAccount MakeCard(string userId = "user-1") =>
        CardAccount.Create(userId, "4321", CardBrand.Visa, CardType.Credit,
            CardNetworkType.Visa, "João Silva", "123.456.789-00", paymentDay: 10);

    [Fact]
    public async Task ExecuteAsync_WithActiveCards_ShouldReturnCardList()
    {
        var cards = new List<CardAccount> { MakeCard(), MakeCard() };
        _repository.Setup(r => r.GetCardAccountsByUserIdAsync("user-1", default))
            .ReturnsAsync(cards);

        var result = await _sut.ExecuteAsync("user-1", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.CardAccounts.Should().HaveCount(2);
        result.Value.CardAccounts.Should().AllSatisfy(c =>
            c.Status.Should().Be(CardAccountStatus.Active));
    }

    [Fact]
    public async Task ExecuteAsync_WithBlockedCard_ShouldExcludeItFromList()
    {
        var activeCard = MakeCard();
        var blockedCard = MakeCard();
        blockedCard.Block();
        _repository.Setup(r => r.GetCardAccountsByUserIdAsync("user-1", default))
            .ReturnsAsync(new List<CardAccount> { activeCard, blockedCard });

        var result = await _sut.ExecuteAsync("user-1", Guid.NewGuid());

        result.Value.CardAccounts.Should().HaveCount(1);
        result.Value.CardAccounts[0].CardAccountId.Should().Be(activeCard.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoCards_ShouldReturnEmptyList()
    {
        _repository.Setup(r => r.GetCardAccountsByUserIdAsync("user-1", default))
            .ReturnsAsync(new List<CardAccount>());

        var result = await _sut.ExecuteAsync("user-1", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.CardAccounts.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenConsentInvalid_ShouldReturnFailure()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.InvalidConsent("Consent is not active."));

        var result = await _sut.ExecuteAsync("user-1", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Consent is not active.");
    }
}
