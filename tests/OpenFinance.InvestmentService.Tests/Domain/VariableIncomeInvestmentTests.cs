using FluentAssertions;
using OpenFinance.InvestmentService.Domain.Entities;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.InvestmentService.Tests.Domain;

public class VariableIncomeInvestmentTests
{
    private static VariableIncomeInvestment ValidStock() =>
        VariableIncomeInvestment.Create(
            "user-1",
            VariableIncomeType.Stock,
            "PETR4",
            "Petrobras",
            "BRPETRACNPR6",
            100m, 30m, 35m, "BRL",
            new DateOnly(2025, 3, 10));

    [Fact]
    public void Create_WithValidData_ShouldCreateInvestment()
    {
        var inv = ValidStock();

        inv.UserId.Should().Be("user-1");
        inv.Type.Should().Be(VariableIncomeType.Stock);
        inv.Ticker.Should().Be("PETR4");
        inv.Quantity.Should().Be(100m);
        inv.AveragePrice.Should().Be(30m);
        inv.CurrentPrice.Should().Be(35m);
    }

    [Fact]
    public void GrossAmount_ShouldBeQuantityTimesCurrentPrice()
    {
        var inv = ValidStock();
        inv.GrossAmount.Should().Be(100m * 35m);  // 3500
    }

    [Fact]
    public void IncomeTax_ShouldBe15PercentOfProfit()
    {
        var inv = ValidStock();
        // Profit = (35 - 30) * 100 = 500
        // IR = 500 * 15% = 75
        inv.IncomeTax.Should().Be(75m);
    }

    [Fact]
    public void IncomeTax_WhenNoProfit_ShouldBeZero()
    {
        var inv = VariableIncomeInvestment.Create(
            "user-1", VariableIncomeType.Stock, "MGLU3", "Magazine Luiza",
            "BRMGLU3CTF001",
            100m, 10m, 8m, "BRL",    // purchased at 10, now worth 8 → loss
            new DateOnly(2025, 3, 10));

        inv.IncomeTax.Should().Be(0m);
    }

    [Fact]
    public void NetAmount_ShouldBeGrossMinusIncomeTax()
    {
        var inv = ValidStock();
        inv.NetAmount.Should().Be(inv.GrossAmount - inv.IncomeTax);
    }

    [Fact]
    public void UpdatePrice_WithNewPrice_ShouldUpdateCurrentPrice()
    {
        var inv = ValidStock();
        inv.UpdatePrice(40m, new DateOnly(2025, 3, 14));

        inv.CurrentPrice.Should().Be(40m);
        inv.LastQuoteDate.Should().Be(new DateOnly(2025, 3, 14));
        inv.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdatePrice_WithNegativePrice_ShouldThrow()
    {
        var inv = ValidStock();
        var act = () => inv.UpdatePrice(-1m, new DateOnly(2025, 3, 14));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithZeroQuantity_ShouldThrow()
    {
        var act = () => VariableIncomeInvestment.Create(
            "user-1", VariableIncomeType.Stock, "PETR4", "Petrobras",
            "BRPETRACNPR6", 0m, 30m, 35m, "BRL", new DateOnly(2025, 3, 10));
        act.Should().Throw<ArgumentException>();
    }
}
