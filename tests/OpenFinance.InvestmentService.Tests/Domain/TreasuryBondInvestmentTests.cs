using FluentAssertions;
using OpenFinance.InvestmentService.Domain.Entities;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.InvestmentService.Tests.Domain;

public class TreasuryBondInvestmentTests
{
    private static TreasuryBondInvestment ValidBond(DateOnly? purchaseDate = null) =>
        TreasuryBondInvestment.Create(
            "user-1",
            "Tesouro Selic 2027",
            TreasuryBondType.Selic,
            purchaseDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-800)),
            new DateOnly(2027, 3, 1),
            2m, 13000m, 14500m, "BRL",
            13.75m, 11.75m);

    [Fact]
    public void Create_WithValidData_ShouldCreateBond()
    {
        var bond = ValidBond();

        bond.UserId.Should().Be("user-1");
        bond.BondType.Should().Be(TreasuryBondType.Selic);
        bond.NominalQuantity.Should().Be(2m);
        bond.NominalUnitPrice.Should().Be(13000m);
        bond.UpdatedUnitPrice.Should().Be(14500m);
    }

    [Fact]
    public void GrossAmount_ShouldBeQuantityTimesUpdatedPrice()
    {
        var bond = ValidBond();
        bond.GrossAmount.Should().Be(2m * 14500m);  // 29000
    }

    [Fact]
    public void IncomeTax_After720Days_ShouldBe15Percent()
    {
        // Purchased > 720 days ago → 15% IR
        var bond = ValidBond(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-800)));
        var profit = bond.GrossAmount - bond.NominalQuantity * bond.NominalUnitPrice;
        var expectedTax = profit * 0.15m;

        bond.IncomeTax.Should().BeApproximately(expectedTax, 0.01m);
    }

    [Fact]
    public void IncomeTax_Within180Days_ShouldBe22Point5Percent()
    {
        var bond = TreasuryBondInvestment.Create(
            "user-1", "Tesouro Prefixado 2026", TreasuryBondType.Prefixado,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90)),
            new DateOnly(2026, 1, 1),
            1m, 1000m, 1100m, "BRL", 13.50m, 0m);

        var profit = bond.GrossAmount - bond.NominalQuantity * bond.NominalUnitPrice;
        var expectedTax = profit * 0.225m;

        bond.IncomeTax.Should().BeApproximately(expectedTax, 0.01m);
    }

    [Fact]
    public void NetAmount_ShouldBeGrossMinusIncomeTax()
    {
        var bond = ValidBond();
        bond.NetAmount.Should().BeApproximately(bond.GrossAmount - bond.IncomeTax, 0.01m);
    }

    [Fact]
    public void Create_WithMaturityBeforePurchase_ShouldThrow()
    {
        var act = () => TreasuryBondInvestment.Create(
            "user-1", "Bond", TreasuryBondType.Selic,
            new DateOnly(2025, 6, 1), new DateOnly(2024, 1, 1),
            1m, 1000m, 1050m, "BRL", 13m, 0m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdatePrice_ShouldRecalculateAmounts()
    {
        var bond = ValidBond();
        var oldGross = bond.GrossAmount;
        bond.UpdatePrice(15000m);

        bond.UpdatedUnitPrice.Should().Be(15000m);
        bond.GrossAmount.Should().NotBe(oldGross);
    }
}
