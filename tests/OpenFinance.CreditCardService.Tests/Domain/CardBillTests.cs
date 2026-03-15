using FluentAssertions;
using OpenFinance.CreditCardService.Domain.Entities;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.CreditCardService.Tests.Domain;

public class CardBillTests
{
    private static CardBill ValidBill() =>
        CardBill.Create(Guid.NewGuid(), new DateOnly(2025, 12, 10), 1500m, 150m, "BRL");

    [Fact]
    public void Create_WithValidData_ShouldCreateOpenBill()
    {
        var bill = ValidBill();

        bill.Status.Should().Be(CardBillStatus.Open);
        bill.TotalAmount.Should().Be(1500m);
        bill.MinimumPaymentAmount.Should().Be(150m);
        bill.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Create_WithNegativeTotal_ShouldThrow()
    {
        var act = () => CardBill.Create(Guid.NewGuid(), new DateOnly(2025, 12, 10), -100m, 0m, "BRL");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithMinimumExceedingTotal_ShouldThrow()
    {
        var act = () => CardBill.Create(Guid.NewGuid(), new DateOnly(2025, 12, 10), 100m, 200m, "BRL");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Close_OpenBill_ShouldSetStatusClosed()
    {
        var bill = ValidBill();
        bill.Close();
        bill.Status.Should().Be(CardBillStatus.Closed);
    }

    [Fact]
    public void Close_AlreadyClosedBill_ShouldThrow()
    {
        var bill = ValidBill();
        bill.Close();
        var act = () => bill.Close();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkPaid_ClosedBill_ShouldSetStatusPaid()
    {
        var bill = ValidBill();
        bill.Close();
        bill.MarkPaid();
        bill.Status.Should().Be(CardBillStatus.Paid);
    }

    [Fact]
    public void MarkOverdue_ClosedBill_ShouldSetStatusOverdue()
    {
        var bill = ValidBill();
        bill.Close();
        bill.MarkOverdue();
        bill.Status.Should().Be(CardBillStatus.Overdue);
    }

    [Fact]
    public void MarkOverdue_OpenBill_ShouldThrow()
    {
        var bill = ValidBill();
        var act = () => bill.MarkOverdue();
        act.Should().Throw<InvalidOperationException>();
    }
}
