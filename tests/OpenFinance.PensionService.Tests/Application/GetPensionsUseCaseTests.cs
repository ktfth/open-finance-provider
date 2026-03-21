using FluentAssertions;
using Moq;
using OpenFinance.PensionService.Application.UseCases;
using OpenFinance.PensionService.Domain.Entities;
using OpenFinance.PensionService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.PensionService.Tests.Application;

public class GetPensionsUseCaseTests
{
    private readonly Mock<IPensionRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetPensionsUseCase _sut;

    public GetPensionsUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetPensionsUseCase(_repository.Object, _consentValidator.Object);
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
            retirementDate: new DateOnly(2050, 1, 15),
            taxRegime: TaxRegimeType.Progressive,
            contributionAmount: 500m,
            contributionFrequency: PaymentFrequency.Monthly,
            currency: "BRL",
            beneficiaryName: "Jane Doe",
            managementFeeRate: 0.015m,
            loadingRate: 0.005m,
            incomeType: IncomeType.LifeAnnuity);

    [Fact]
    public async Task ExecuteAsync_WhenUserHasPensions_ShouldReturnMappedSummaries()
    {
        var pension = ValidPension();
        _repository.Setup(r => r.GetByUserIdAsync("user-001", default))
            .ReturnsAsync(new[] { pension });

        var result = await _sut.ExecuteAsync("user-001", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Pensions.Should().HaveCount(1);
        result.Value.Pensions[0].ProductName.Should().Be("VGBL Progressivo");
        result.Value.Pensions[0].Type.Should().Be(PensionType.VGBL);
        result.Value.Pensions[0].CertificateNumber.Should().Be("CERT-2024-001");
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserHasNoPensions_ShouldReturnEmptyList()
    {
        _repository.Setup(r => r.GetByUserIdAsync("user-002", default))
            .ReturnsAsync(Array.Empty<Pension>());

        var result = await _sut.ExecuteAsync("user-002", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Pensions.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserHasMultiplePensions_ShouldReturnAllMapped()
    {
        var pension1 = ValidPension();
        var pension2 = Pension.Create(
            "user-001", PensionType.PGBL, PensionModality.Contribution,
            "PGBL Regressivo", "Previdência XYZ", "98.765.432/0001-11",
            "CERT-2024-002", new DateOnly(2018, 6, 1), null,
            TaxRegimeType.Regressive, 1000m, PaymentFrequency.Monthly,
            "BRL", "John Doe", 0.012m, 0.003m, IncomeType.LumpSum);

        _repository.Setup(r => r.GetByUserIdAsync("user-001", default))
            .ReturnsAsync(new[] { pension1, pension2 });

        var result = await _sut.ExecuteAsync("user-001", Guid.NewGuid());

        result.Value.Pensions.Should().HaveCount(2);
    }
}
