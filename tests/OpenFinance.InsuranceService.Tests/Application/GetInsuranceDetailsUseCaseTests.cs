using FluentAssertions;
using Moq;
using OpenFinance.InsuranceService.Application.UseCases;
using OpenFinance.InsuranceService.Domain.Entities;
using OpenFinance.InsuranceService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.InsuranceService.Tests.Application;

public class GetInsuranceDetailsUseCaseTests
{
    private readonly Mock<IInsuranceRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetInsuranceDetailsUseCase _sut;

    public GetInsuranceDetailsUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetInsuranceDetailsUseCase(_repository.Object, _consentValidator.Object);
    }

    private static Insurance ValidInsurance() =>
        Insurance.Create(
            userId: "user-001",
            type: InsuranceType.Auto,
            productName: "Seguro Auto Total",
            insurerName: "Seguradora XYZ S/A",
            insurerCnpj: "98.765.432/0001-11",
            policyNumber: "POL-AUTO-2024",
            proposalDate: new DateOnly(2024, 1, 10),
            effectiveDate: new DateOnly(2024, 2, 1),
            expirationDate: new DateOnly(2025, 2, 1),
            insuredAmount: 80_000m,
            premiumAmount: 250m,
            currency: "BRL",
            gracePeriodDays: 0,
            insuredCpfCnpj: "111.222.333-44",
            insuredName: "Maria Silva",
            beneficiaryName: "Maria Silva");

    [Fact]
    public async Task ExecuteAsync_WhenInsuranceExists_ShouldReturnDetailedResponse()
    {
        var insurance = ValidInsurance();
        _repository.Setup(r => r.GetByIdAsync(insurance.Id, default))
            .ReturnsAsync(insurance);

        var result = await _sut.ExecuteAsync(insurance.Id, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ProductName.Should().Be("Seguro Auto Total");
        result.Value.InsuredAmount.Should().Be(80_000m);
        result.Value.PremiumAmount.Should().Be(250m);
        result.Value.InsuredName.Should().Be("Maria Silva");
    }

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
}
