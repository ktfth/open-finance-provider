using FluentAssertions;
using Moq;
using OpenFinance.CapitalizationService.Application.UseCases;
using OpenFinance.CapitalizationService.Domain.Entities;
using OpenFinance.CapitalizationService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.CapitalizationService.Tests.Application;

public class GetCapitalizationBondsUseCaseTests
{
    private readonly Mock<ICapitalizationRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetCapitalizationBondsUseCase _sut;

    public GetCapitalizationBondsUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetCapitalizationBondsUseCase(_repository.Object, _consentValidator.Object);
    }

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
    public async Task ExecuteAsync_WhenUserHasBonds_ShouldReturnMappedSummaries()
    {
        var bond = ValidBond();
        _repository.Setup(r => r.GetByUserIdAsync("user-001", default))
            .ReturnsAsync(new[] { bond });

        var result = await _sut.ExecuteAsync("user-001", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Bonds.Should().HaveCount(1);
        result.Value.Bonds[0].BondNumber.Should().Be("CAP-2024-001");
        result.Value.Bonds[0].Modality.Should().Be(CapitalizationModality.Traditional);
        result.Value.Bonds[0].ProductName.Should().Be("Capitalização Premiada");
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserHasNoBonds_ShouldReturnEmptyList()
    {
        _repository.Setup(r => r.GetByUserIdAsync("user-002", default))
            .ReturnsAsync(Array.Empty<CapitalizationBond>());

        var result = await _sut.ExecuteAsync("user-002", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Bonds.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserHasMultipleBonds_ShouldReturnAllMapped()
    {
        var bond1 = ValidBond();
        var bond2 = CapitalizationBond.Create(
            "user-001", "CAP-2024-002", CapitalizationModality.Popular,
            "Capitalização Popular", "Seguradora XYZ", "98.765.432/0001-11",
            new DateOnly(2024, 3, 1), new DateOnly(2025, 3, 1),
            12, 50m, PaymentFrequency.Monthly,
            0.02m, 0.01m, 0.85m, 400m, 5_000m, "BRL", 200m, 195m, 0.80m);

        _repository.Setup(r => r.GetByUserIdAsync("user-001", default))
            .ReturnsAsync(new[] { bond1, bond2 });

        var result = await _sut.ExecuteAsync("user-001", Guid.NewGuid());

        result.Value.Bonds.Should().HaveCount(2);
        result.Value.Bonds.Should().Contain(b => b.Modality == CapitalizationModality.Popular);
    }
}
