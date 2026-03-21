using FluentAssertions;
using OpenFinance.PensionService.Domain.Entities;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.PensionService.Tests.Domain;

public class PensionTests
{
    private static Pension ValidPension() =>
        Pension.Create(
            userId: "user-001",
            type: PensionType.PGBL,
            modality: PensionModality.Contribution,
            productName: "PGBL Regressivo",
            insurerName: "Previdência XYZ",
            insurerCnpj: "98.765.432/0001-11",
            certificateNumber: "CERT-001",
            contractDate: new DateOnly(2015, 3, 1),
            retirementDate: new DateOnly(2045, 3, 1),
            taxRegime: TaxRegimeType.Regressive,
            contributionAmount: 800m,
            contributionFrequency: PaymentFrequency.Monthly,
            currency: "BRL",
            beneficiaryName: "Jane Doe",
            managementFeeRate: 0.01m,
            loadingRate: 0.002m,
            incomeType: IncomeType.LumpSum);

    [Fact]
    public void Create_WithValidData_ShouldCreateActivePension()
    {
        var pension = ValidPension();

        pension.Status.Should().Be(PensionStatus.Active);
        pension.ProductName.Should().Be("PGBL Regressivo");
        pension.Type.Should().Be(PensionType.PGBL);
        pension.ContributionAmount.Should().Be(800m);
    }

    [Fact]
    public void Suspend_WhenActive_ShouldChangeToPendingStatus()
    {
        var pension = ValidPension();
        pension.Suspend();

        pension.Status.Should().Be(PensionStatus.Suspended);
    }

    [Fact]
    public void Cancel_WhenActive_ShouldSetCancelledStatus()
    {
        var pension = ValidPension();
        pension.Cancel();

        pension.Status.Should().Be(PensionStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrow()
    {
        var pension = ValidPension();
        pension.Cancel();

        var act = () => pension.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Redeem_WhenActive_ShouldSetRedeemedStatus()
    {
        var pension = ValidPension();
        pension.Redeem();

        pension.Status.Should().Be(PensionStatus.Redeemed);
    }

    [Fact]
    public void Create_WithNegativeContributionAmount_ShouldThrow()
    {
        var act = () => Pension.Create(
            "user-001", PensionType.VGBL, PensionModality.Contribution,
            "Product", "Insurer", "12.345.678/0001-99", "CERT-001",
            new DateOnly(2020, 1, 1), null, TaxRegimeType.Progressive,
            -100m, PaymentFrequency.Monthly, "BRL", "Beneficiary", 0.01m, 0.002m, IncomeType.LumpSum);

        act.Should().Throw<ArgumentException>().WithMessage("*Contribution*");
    }
}
