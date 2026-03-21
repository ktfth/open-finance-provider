using FluentAssertions;
using Moq;
using OpenFinance.CapitalizationService.Application.UseCases;
using OpenFinance.CapitalizationService.Domain.Entities;
using OpenFinance.CapitalizationService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.CapitalizationService.Tests.Application;

public class GetCapitalizationBondDetailsUseCaseTests
{
    private readonly Mock<ICapitalizationRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetCapitalizationBondDetailsUseCase _sut;

    public GetCapitalizationBondDetailsUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetCapitalizationBondDetailsUseCase(_repository.Object, _consentValidator.Object);
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
    public async Task ExecuteAsync_WhenBondExists_ShouldReturnDetailedResponse()
    {
        var bond = ValidBond();
        _repository.Setup(r => r.GetByIdAsync(bond.Id, default))
            .ReturnsAsync(bond);

        var result = await _sut.ExecuteAsync(bond.Id, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.BondNumber.Should().Be("CAP-2024-001");
        result.Value.PaymentCount.Should().Be(24);
        result.Value.PaymentAmount.Should().Be(100m);
        result.Value.PrizeDrawAmount.Should().Be(10_000m);
        result.Value.MathematicalReserve.Should().Be(870m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBondNotFound_ShouldReturnSuccessWithNull()
    {
        var missingId = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(missingId, default))
            .ReturnsAsync((CapitalizationBond?)null);

        var result = await _sut.ExecuteAsync(missingId, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenBondExists_ShouldReturnActiveStatus()
    {
        var bond = ValidBond();
        _repository.Setup(r => r.GetByIdAsync(bond.Id, default))
            .ReturnsAsync(bond);

        var result = await _sut.ExecuteAsync(bond.Id, Guid.NewGuid());

        result.Value!.Status.Should().Be(CapitalizationBondStatus.Active);
        result.Value.Currency.Should().Be("BRL");
    }
}
