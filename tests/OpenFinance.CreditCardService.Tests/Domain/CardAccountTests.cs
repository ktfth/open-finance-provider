using FluentAssertions;
using OpenFinance.CreditCardService.Domain.Entities;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.CreditCardService.Tests.Domain;

public class CardAccountTests
{
    private static CardAccount ValidCard() =>
        CardAccount.Create("user-1", "4321", CardBrand.Visa, CardType.Credit,
            CardNetworkType.Visa, "João Silva", "123.456.789-00", paymentDay: 10);

    [Fact]
    public void Create_WithValidData_ShouldCreateActiveCard()
    {
        var card = ValidCard();

        card.UserId.Should().Be("user-1");
        card.LastFourDigits.Should().Be("4321");
        card.Brand.Should().Be(CardBrand.Visa);
        card.CardType.Should().Be(CardType.Credit);
        card.Status.Should().Be(CardAccountStatus.Active);
        card.PaymentDay.Should().Be(10);
        card.IsActive().Should().BeTrue();
        card.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("123")]      // too short
    [InlineData("12345")]    // too long
    [InlineData("12ab")]     // non-numeric
    [InlineData("")]
    public void Create_WithInvalidLastFourDigits_ShouldThrow(string digits)
    {
        var act = () => CardAccount.Create("user-1", digits, CardBrand.Visa, CardType.Credit,
            CardNetworkType.Visa, "João Silva", "123.456.789-00", paymentDay: 10);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    [InlineData(-1)]
    public void Create_WithInvalidPaymentDay_ShouldThrow(int day)
    {
        var act = () => CardAccount.Create("user-1", "4321", CardBrand.Visa, CardType.Credit,
            CardNetworkType.Visa, "João Silva", "123.456.789-00", paymentDay: day);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null!)]
    public void Create_WithMissingUserId_ShouldThrow(string userId)
    {
        var act = () => CardAccount.Create(userId, "4321", CardBrand.Visa, CardType.Credit,
            CardNetworkType.Visa, "João Silva", "123.456.789-00", paymentDay: 10);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Block_ActiveCard_ShouldSetStatusBlocked()
    {
        var card = ValidCard();
        card.Block();

        card.Status.Should().Be(CardAccountStatus.Blocked);
        card.IsActive().Should().BeFalse();
        card.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Unblock_BlockedCard_ShouldSetStatusActive()
    {
        var card = ValidCard();
        card.Block();
        card.Unblock();

        card.Status.Should().Be(CardAccountStatus.Active);
        card.IsActive().Should().BeTrue();
    }

    [Fact]
    public void Unblock_ActiveCard_ShouldThrow()
    {
        var card = ValidCard();
        var act = () => card.Unblock();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_ActiveCard_ShouldSetStatusCancelled()
    {
        var card = ValidCard();
        card.Cancel();

        card.Status.Should().Be(CardAccountStatus.Cancelled);
        card.IsActive().Should().BeFalse();
    }

    [Fact]
    public void Cancel_AlreadyCancelledCard_ShouldThrow()
    {
        var card = ValidCard();
        card.Cancel();
        var act = () => card.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Block_CancelledCard_ShouldThrow()
    {
        var card = ValidCard();
        card.Cancel();
        var act = () => card.Block();
        act.Should().Throw<InvalidOperationException>();
    }
}
