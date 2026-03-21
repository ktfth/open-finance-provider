using FluentAssertions;
using Moq;
using OpenFinance.InvestmentService.Application.UseCases;
using OpenFinance.InvestmentService.Domain.Entities;
using OpenFinance.InvestmentService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.InvestmentService.Tests.Application;

public class GetTreasuryBondsUseCaseTests
{
    private readonly Mock<IInvestmentRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetTreasuryBondsUseCase _sut;

    public GetTreasuryBondsUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetTreasuryBondsUseCase(_repository.Object, _consentValidator.Object);
    }

    private static TreasuryBondInvestment ValidBond(string userId = "user-1") =>
        TreasuryBondInvestment.Create(
            userId,
            "Tesouro Selic 2027",
            TreasuryBondType.Selic,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-400)),
            new DateOnly(2027, 3, 1),
            2m, 13456.78m, 14102.33m, "BRL", 13.75m, 11.75m);

    [Fact]
    public async Task ExecuteAsync_WithBonds_ShouldReturnSummaryList()
    {
        var bonds = new List<TreasuryBondInvestment> { ValidBond(), ValidBond() };
        _repository.Setup(r => r.GetTreasuryBondsByUserIdAsync("user-1", default)).ReturnsAsync(bonds);

        var result = await _sut.ExecuteAsync("user-1", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Bonds.Should().HaveCount(2);
        result.Value.Bonds.Should().AllSatisfy(b =>
        {
            b.ProductName.Should().Be("Tesouro Selic 2027");
            b.BondType.Should().Be(TreasuryBondType.Selic);
            b.CurrentAmount.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public async Task ExecuteAsync_WithNoBonds_ShouldReturnEmptyList()
    {
        _repository.Setup(r => r.GetTreasuryBondsByUserIdAsync("user-1", default))
            .ReturnsAsync(new List<TreasuryBondInvestment>());

        var result = await _sut.ExecuteAsync("user-1", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Bonds.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_InvestedAmountShouldBeNominalQuantityTimesNominalPrice()
    {
        var bond = ValidBond();
        _repository.Setup(r => r.GetTreasuryBondsByUserIdAsync("user-1", default))
            .ReturnsAsync(new List<TreasuryBondInvestment> { bond });

        var result = await _sut.ExecuteAsync("user-1", Guid.NewGuid());

        var summary = result.Value.Bonds[0];
        summary.InvestedAmount.Should().Be(bond.NominalQuantity * bond.NominalUnitPrice);
        summary.CurrentAmount.Should().Be(bond.GrossAmount);
    }
}
