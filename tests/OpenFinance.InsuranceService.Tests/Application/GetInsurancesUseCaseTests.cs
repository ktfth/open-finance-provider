using FluentAssertions;
using Moq;
using OpenFinance.InsuranceService.Application.UseCases;
using OpenFinance.InsuranceService.Domain.Entities;
using OpenFinance.InsuranceService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.InsuranceService.Tests.Application;

public class GetInsurancesUseCaseTests
{
    private readonly Mock<IInsuranceRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetInsurancesUseCase _sut;

    public GetInsurancesUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetInsurancesUseCase(_repository.Object, _consentValidator.Object);
    }

    private static Insurance ValidInsurance() =>
        Insurance.Create(
            userId: "user-001",
            type: InsuranceType.Life,
            productName: "Vida Família Plus",
            insurerName: "Seguradora ABC S/A",
            insurerCnpj: "12.345.678/0001-99",
            policyNumber: "POL-2024-001",
            proposalDate: new DateOnly(2024, 1, 5),
            effectiveDate: new DateOnly(2024, 2, 1),
            expirationDate: new DateOnly(2025, 2, 1),
            insuredAmount: 500_000m,
            premiumAmount: 150m,
            currency: "BRL",
            gracePeriodDays: 30,
            insuredCpfCnpj: "123.456.789-00",
            insuredName: "John Doe",
            beneficiaryName: "Jane Doe");

    [Fact]
    public async Task ExecuteAsync_WhenUserHasInsurances_ShouldReturnMappedSummaries()
    {
        var insurance = ValidInsurance();
        _repository.Setup(r => r.GetByUserIdAsync("user-001", default))
            .ReturnsAsync(new[] { insurance });

        var result = await _sut.ExecuteAsync("user-001", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Insurances.Should().HaveCount(1);
        result.Value.Insurances[0].ProductName.Should().Be("Vida Família Plus");
        result.Value.Insurances[0].Type.Should().Be(InsuranceType.Life);
        result.Value.Insurances[0].PolicyNumber.Should().Be("POL-2024-001");
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserHasNoInsurances_ShouldReturnEmptyList()
    {
        _repository.Setup(r => r.GetByUserIdAsync("user-002", default))
            .ReturnsAsync(Array.Empty<Insurance>());

        var result = await _sut.ExecuteAsync("user-002", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Insurances.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserHasMultipleInsurances_ShouldReturnAllMapped()
    {
        var insurance1 = ValidInsurance();
        var insurance2 = Insurance.Create(
            "user-001", InsuranceType.Home, "Seguro Residencial", "Seguradora ABC S/A",
            "12.345.678/0001-99", "POL-2024-002",
            new DateOnly(2024, 3, 1), new DateOnly(2024, 4, 1), new DateOnly(2025, 4, 1),
            200_000m, 80m, "BRL", 15, "123.456.789-00", "John Doe", "Jane Doe");

        _repository.Setup(r => r.GetByUserIdAsync("user-001", default))
            .ReturnsAsync(new[] { insurance1, insurance2 });

        var result = await _sut.ExecuteAsync("user-001", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Insurances.Should().HaveCount(2);
        result.Value.Insurances.Should().Contain(i => i.Type == InsuranceType.Life);
        result.Value.Insurances.Should().Contain(i => i.Type == InsuranceType.Home);
    }
}
