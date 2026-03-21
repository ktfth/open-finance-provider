using FluentAssertions;
using Moq;
using OpenFinance.InsuranceService.Application.UseCases;
using OpenFinance.InsuranceService.Domain.Entities;
using OpenFinance.InsuranceService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using DomainPremiumPayment = OpenFinance.InsuranceService.Domain.Entities.InsurancePremiumPayment;

namespace OpenFinance.InsuranceService.Tests.Application;

public class GetInsurancePremiumUseCaseTests
{
    private readonly Mock<IInsuranceRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetInsurancePremiumUseCase _sut;

    public GetInsurancePremiumUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetInsurancePremiumUseCase(_repository.Object, _consentValidator.Object);
    }

    private static Insurance ValidInsurance() =>
        Insurance.Create(
            userId: "user-001",
            type: InsuranceType.Life,
            productName: "Vida Plus",
            insurerName: "Seguradora ABC",
            insurerCnpj: "12.345.678/0001-99",
            policyNumber: "POL-001",
            proposalDate: new DateOnly(2024, 1, 1),
            effectiveDate: new DateOnly(2024, 2, 1),
            expirationDate: new DateOnly(2025, 2, 1),
            insuredAmount: 300_000m,
            premiumAmount: 120m,
            currency: "BRL",
            gracePeriodDays: 30,
            insuredCpfCnpj: "123.456.789-00",
            insuredName: "John Doe",
            beneficiaryName: "Jane Doe");

    private static DomainPremiumPayment PendingPayment(Guid insuranceId) =>
        DomainPremiumPayment.Create(insuranceId, new DateOnly(2024, 2, 1), 120m, "BRL");

    [Fact]
    public async Task ExecuteAsync_WhenInsuranceNotFound_ShouldReturnSuccessWithNull()
    {
        var missingId = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(missingId, default))
            .ReturnsAsync((Insurance?)null);

        var result = await _sut.ExecuteAsync(missingId, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenInsuranceHasPaidAndPendingPayments_ShouldCalculateTotalsCorrectly()
    {
        var insurance = ValidInsurance();
        var payment1 = PendingPayment(insurance.Id);
        payment1.MarkPaid();
        var payment2 = PendingPayment(insurance.Id);

        _repository.Setup(r => r.GetByIdAsync(insurance.Id, default)).ReturnsAsync(insurance);
        _repository.Setup(r => r.GetPremiumPaymentsAsync(insurance.Id, default))
            .ReturnsAsync(new DomainPremiumPayment[] { payment1, payment2 });

        var result = await _sut.ExecuteAsync(insurance.Id, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalPremium.Should().Be(240m);
        result.Value.PaidAmount.Should().Be(120m);
        result.Value.OutstandingAmount.Should().Be(120m);
        result.Value.Payments.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInsuranceHasNoPayments_ShouldReturnZeroTotals()
    {
        var insurance = ValidInsurance();

        _repository.Setup(r => r.GetByIdAsync(insurance.Id, default)).ReturnsAsync(insurance);
        _repository.Setup(r => r.GetPremiumPaymentsAsync(insurance.Id, default))
            .ReturnsAsync(Array.Empty<DomainPremiumPayment>());

        var result = await _sut.ExecuteAsync(insurance.Id, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalPremium.Should().Be(0m);
        result.Value.PaidAmount.Should().Be(0m);
        result.Value.OutstandingAmount.Should().Be(0m);
        result.Value.Payments.Should().BeEmpty();
    }
}
