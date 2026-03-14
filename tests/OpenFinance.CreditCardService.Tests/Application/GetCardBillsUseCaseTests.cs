using FluentAssertions;
using Moq;
using OpenFinance.CreditCardService.Application.UseCases;
using OpenFinance.CreditCardService.Domain.Entities;
using OpenFinance.CreditCardService.Domain.Repositories;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.CreditCardService.Tests.Application;

public class GetCardBillsUseCaseTests
{
    private readonly Mock<ICardRepository> _repository = new();
    private readonly GetCardBillsUseCase _sut;

    public GetCardBillsUseCaseTests() =>
        _sut = new GetCardBillsUseCase(_repository.Object);

    private static CardAccount MakeCard() =>
        CardAccount.Create("user-1", "4321", CardBrand.Visa, CardType.Credit,
            CardNetworkType.Visa, "João Silva", "123.456.789-00", paymentDay: 10);

    [Fact]
    public async Task ExecuteAsync_WithExistingBills_ShouldReturnBillList()
    {
        var card = MakeCard();
        var bills = new List<CardBill>
        {
            CardBill.Create(card.Id, new DateOnly(2025, 1, 10), 1200m, 120m, "BRL"),
            CardBill.Create(card.Id, new DateOnly(2025, 2, 10), 800m, 80m, "BRL"),
        };

        _repository.Setup(r => r.GetCardAccountByIdAsync(card.Id, default)).ReturnsAsync(card);
        _repository.Setup(r => r.GetCardBillsAsync(card.Id, default)).ReturnsAsync(bills);

        var result = await _sut.ExecuteAsync(card.Id, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Bills.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCardNotFound_ShouldReturnFailure()
    {
        _repository.Setup(r => r.GetCardAccountByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((CardAccount?)null);

        var result = await _sut.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task ExecuteAsync_WithNoBills_ShouldReturnEmptyList()
    {
        var card = MakeCard();
        _repository.Setup(r => r.GetCardAccountByIdAsync(card.Id, default)).ReturnsAsync(card);
        _repository.Setup(r => r.GetCardBillsAsync(card.Id, default))
            .ReturnsAsync(new List<CardBill>());

        var result = await _sut.ExecuteAsync(card.Id, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Bills.Should().BeEmpty();
    }
}
