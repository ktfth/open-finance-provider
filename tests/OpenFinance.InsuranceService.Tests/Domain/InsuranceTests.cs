using FluentAssertions;
using OpenFinance.InsuranceService.Domain.Entities;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.InsuranceService.Tests.Domain;

public class InsuranceTests
{
    private static Insurance ValidInsurance() =>
        Insurance.Create(
            userId: "user-001",
            type: InsuranceType.Life,
            productName: "Vida Familia",
            insurerName: "Seguradora ABC",
            insurerCnpj: "12.345.678/0001-99",
            policyNumber: "POL-2024-001",
            proposalDate: new DateOnly(2024, 1, 1),
            effectiveDate: new DateOnly(2024, 2, 1),
            expirationDate: new DateOnly(2025, 2, 1),
            insuredAmount: 400_000m,
            premiumAmount: 100m,
            currency: "BRL",
            gracePeriodDays: 30,
            insuredCpfCnpj: "123.456.789-00",
            insuredName: "John Doe",
            beneficiaryName: "Jane Doe");

    [Fact]
    public void Create_WithValidData_ShouldCreatePendingActivationInsurance()
    {
        var insurance = ValidInsurance();

        insurance.Status.Should().Be(InsuranceStatus.PendingActivation);
        insurance.InsuredAmount.Should().Be(400_000m);
        insurance.Type.Should().Be(InsuranceType.Life);
    }

    [Fact]
    public void Activate_WhenPendingActivation_ShouldSetActiveStatus()
    {
        var insurance = ValidInsurance();
        insurance.Activate();

        insurance.Status.Should().Be(InsuranceStatus.Active);
        insurance.IsActive().Should().BeTrue();
    }

    [Fact]
    public void Cancel_WhenActive_ShouldSetCancelledStatus()
    {
        var insurance = ValidInsurance();
        insurance.Activate();
        insurance.Cancel();

        insurance.Status.Should().Be(InsuranceStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrow()
    {
        var insurance = ValidInsurance();
        insurance.Activate();
        insurance.Cancel();

        var act = () => insurance.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Create_WithExpirationBeforeEffectiveDate_ShouldThrow()
    {
        var act = () => Insurance.Create(
            "user-001", InsuranceType.Life, "Product", "Insurer", "12.345.678/0001-99",
            "POL-001", new DateOnly(2024, 1, 1), new DateOnly(2024, 6, 1), new DateOnly(2024, 3, 1),
            100_000m, 50m, "BRL", 0, "123.456.789-00", "John", "Jane");

        act.Should().Throw<ArgumentException>().WithMessage("*Expiration*");
    }
}
