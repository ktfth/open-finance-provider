using FluentAssertions;
using OpenFinance.InvestmentService.Domain.Entities;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.InvestmentService.Tests.Domain;

public class FixedIncomeInvestmentTests
{
    private static FixedIncomeInvestment ValidCDB() =>
        FixedIncomeInvestment.Create(
            "user-1",
            FixedIncomeType.CDB,
            "CDB 120% CDI",
            "Banco Teste",
            "BRTST11CDB001",
            new DateOnly(2024, 1, 1),
            new DateOnly(2025, 1, 1),
            1000m, 1000m, 5m, "BRL",
            RateIndexer.CDI, 120m, 0m, 0m, 0m,
            RemunType.PostFixed);

    [Fact]
    public void Create_WithValidData_ShouldCreateInvestment()
    {
        var inv = ValidCDB();

        inv.UserId.Should().Be("user-1");
        inv.Type.Should().Be(FixedIncomeType.CDB);
        inv.ProductName.Should().Be("CDB 120% CDI");
        inv.Quantity.Should().Be(5m);
        inv.PurchaseUnitPrice.Should().Be(1000m);
        inv.GrossAmount.Should().Be(5000m);
        inv.Indexer.Should().Be(RateIndexer.CDI);
        inv.IndexerPercentage.Should().Be(120m);
    }

    [Fact]
    public void Create_WithZeroQuantity_ShouldThrow()
    {
        var act = () => FixedIncomeInvestment.Create(
            "user-1", FixedIncomeType.CDB, "CDB", "Bank", "ISIN001",
            new DateOnly(2024, 1, 1), new DateOnly(2025, 1, 1),
            1000m, 1000m, 0m, "BRL",
            RateIndexer.CDI, 100m, 0m, 0m, 0m, RemunType.PostFixed);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithMaturityBeforeIssue_ShouldThrow()
    {
        var act = () => FixedIncomeInvestment.Create(
            "user-1", FixedIncomeType.CDB, "CDB", "Bank", "ISIN001",
            new DateOnly(2025, 6, 1), new DateOnly(2024, 1, 1),
            1000m, 1000m, 1m, "BRL",
            RateIndexer.CDI, 100m, 0m, 0m, 0m, RemunType.PostFixed);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GrossAmount_ShouldBeQuantityTimesUnitPrice()
    {
        var inv = ValidCDB();
        inv.GrossAmount.Should().Be(inv.Quantity * inv.PurchaseUnitPrice);
    }

    [Fact]
    public void Create_LCI_WithTaxExemption_ShouldSetExemptionTo100Percent()
    {
        var lci = FixedIncomeInvestment.Create(
            "user-1", FixedIncomeType.LCI, "LCI 95% CDI", "Banco",
            "BRLCI11001",
            new DateOnly(2024, 1, 1), new DateOnly(2025, 6, 1),
            1000m, 1000m, 10m, "BRL",
            RateIndexer.CDI, 95m, 0m, 0m, 100m,
            RemunType.PostFixed);

        lci.TaxExemptionPercentage.Should().Be(100m);
    }
}
