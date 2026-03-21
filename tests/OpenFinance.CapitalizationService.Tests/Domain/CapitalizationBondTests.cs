using FluentAssertions;
using OpenFinance.CapitalizationService.Domain.Entities;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.CapitalizationService.Tests.Domain;

public class CapitalizationBondTests
{
    private static CapitalizationBond ValidBond() =>
        CapitalizationBond.Create(
            userId: "user-001",
            bondNumber: "CAP-2024-001",
            modality: CapitalizationModality.Traditional,
            productName: "Capitalização Premiada",
            companyName: "Seguradora Cap S/A",
            companyCnpj: "12.345.678/0001-99",
            contractDate: new DateOnly(2024, 1, 10),
            maturityDate: new DateOnly(2026, 1, 10),
            paymentCount: 24,
            paymentAmount: 100m,
            paymentFrequency: PaymentFrequency.Monthly,
            latePaymentFine: 0.02m,
            latePaymentInterest: 0.01m,
            redemptionPercentage: 0.90m,
            currentRedemptionValue: 800m,
            prizeDrawAmount: 10_000m,
            currency: "BRL",
            totalPaidAmount: 900m,
            mathematicalReserve: 870m,
            surrenderQuota: 0.85m);

    [Fact]
    public void Create_WithValidData_ShouldCreateActiveBond()
    {
        var bond = ValidBond();

        bond.Status.Should().Be(CapitalizationBondStatus.Active);
        bond.BondNumber.Should().Be("CAP-2024-001");
        bond.PaymentCount.Should().Be(24);
        bond.PaymentAmount.Should().Be(100m);
    }

    [Fact]
    public void Redeem_WhenActive_ShouldSetRedeemedStatus()
    {
        var bond = ValidBond();
        bond.Redeem();

        bond.Status.Should().Be(CapitalizationBondStatus.Redeemed);
    }

    [Fact]
    public void Suspend_WhenActive_ShouldSetSuspendedStatus()
    {
        var bond = ValidBond();
        bond.Suspend();

        bond.Status.Should().Be(CapitalizationBondStatus.Suspended);
    }

    [Fact]
    public void Expire_WhenActive_ShouldSetExpiredStatus()
    {
        var bond = ValidBond();
        bond.Expire();

        bond.Status.Should().Be(CapitalizationBondStatus.Expired);
    }

    [Fact]
    public void Expire_WhenAlreadyRedeemed_ShouldThrow()
    {
        var bond = ValidBond();
        bond.Redeem();

        var act = () => bond.Expire();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Create_WithMaturityBeforeContractDate_ShouldThrow()
    {
        var act = () => CapitalizationBond.Create(
            "user-001", "CAP-001", CapitalizationModality.Traditional,
            "Product", "Company", "12.345.678/0001-99",
            contractDate: new DateOnly(2024, 6, 1),
            maturityDate: new DateOnly(2024, 3, 1),
            12, 50m, PaymentFrequency.Monthly,
            0.02m, 0.01m, 0.85m, 400m, 5_000m, "BRL", 200m, 195m, 0.80m);

        act.Should().Throw<ArgumentException>().WithMessage("*Maturity*");
    }

    [Fact]
    public void Create_WithZeroPaymentCount_ShouldThrow()
    {
        var act = () => CapitalizationBond.Create(
            "user-001", "CAP-001", CapitalizationModality.Traditional,
            "Product", "Company", "12.345.678/0001-99",
            new DateOnly(2024, 1, 1), new DateOnly(2026, 1, 1),
            0, 50m, PaymentFrequency.Monthly,
            0.02m, 0.01m, 0.85m, 400m, 5_000m, "BRL", 200m, 195m, 0.80m);

        act.Should().Throw<ArgumentException>().WithMessage("*Payment count*");
    }
}
