using FluentAssertions;
using Moq;
using OpenFinance.PensionService.Application.UseCases;
using OpenFinance.PensionService.Domain.Entities;
using OpenFinance.PensionService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.PensionService.Tests.Application;

public class GetPensionBalanceUseCaseTests
{
    private readonly Mock<IPensionRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetPensionBalanceUseCase _sut;

    public GetPensionBalanceUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetPensionBalanceUseCase(_repository.Object, _consentValidator.Object);
    }

    private static Pension ValidPension() =>
        Pension.Create(
            userId: "user-001",
            type: PensionType.VGBL,
            modality: PensionModality.Contribution,
            productName: "VGBL Progressivo",
            insurerName: "Previdência ABC",
            insurerCnpj: "12.345.678/0001-99",
            certificateNumber: "CERT-2024-001",
            contractDate: new DateOnly(2020, 1, 15),
            retirementDate: null,
            taxRegime: TaxRegimeType.Progressive,
            contributionAmount: 500m,
            contributionFrequency: PaymentFrequency.Monthly,
            currency: "BRL",
            beneficiaryName: "Jane Doe",
            managementFeeRate: 0.015m,
            loadingRate: 0.005m,
            incomeType: IncomeType.LifeAnnuity);

    [Fact]
    public async Task ExecuteAsync_WhenPensionAndBalanceExist_ShouldReturnBalanceResponse()
    {
        var pension = ValidPension();
        var balance = PensionBalance.Create(
            pension.Id,
            new DateOnly(2024, 12, 31),
            grossBalance: 62_500m,
            netBalance: 58_000m,
            totalContributions: 50_000m,
            totalYield: 12_500m,
            managementFee: 750m,
            loadingFee: 250m,
            currency: "BRL");

        _repository.Setup(r => r.GetByIdAsync(pension.Id, default)).ReturnsAsync(pension);
        _repository.Setup(r => r.GetBalanceAsync(pension.Id, default)).ReturnsAsync(balance);

        var result = await _sut.ExecuteAsync(pension.Id, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.GrossBalance.Should().Be(62_500m);
        result.Value.NetBalance.Should().Be(58_000m);
        result.Value.TotalContributions.Should().Be(50_000m);
        result.Value.TotalYield.Should().Be(12_500m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPensionNotFound_ShouldReturnSuccessWithNull()
    {
        var missingId = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(missingId, default))
            .ReturnsAsync((Pension?)null);

        var result = await _sut.ExecuteAsync(missingId, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
        _repository.Verify(r => r.GetBalanceAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPensionExistsButNoBalance_ShouldReturnSuccessWithNull()
    {
        var pension = ValidPension();

        _repository.Setup(r => r.GetByIdAsync(pension.Id, default)).ReturnsAsync(pension);
        _repository.Setup(r => r.GetBalanceAsync(pension.Id, default))
            .ReturnsAsync((PensionBalance?)null);

        var result = await _sut.ExecuteAsync(pension.Id, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }
}
