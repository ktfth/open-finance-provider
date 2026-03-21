using FluentAssertions;
using Moq;
using OpenFinance.LoanService.Application.UseCases;
using OpenFinance.LoanService.Domain.Entities;
using OpenFinance.LoanService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.LoanService.Tests.Application;

public class GetOverdraftDetailsUseCaseTests
{
    private readonly Mock<ILoanRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetOverdraftDetailsUseCase _sut;

    public GetOverdraftDetailsUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetOverdraftDetailsUseCase(_repository.Object, _consentValidator.Object);
    }

    private static OverdraftContract ValidOverdraftContract() =>
        OverdraftContract.Create(
            userId: "user-001",
            contractNumber: "OD-2024-001",
            companyCnpj: "12.345.678/0001-99",
            contractAmount: 5_000m,
            outstandingBalance: 3_200m,
            interestRate: 0.035m,
            interestRateType: InterestRateType.Simple,
            indexer: RateIndexer.CDI,
            currency: "BRL",
            contractDate: new DateOnly(2024, 2, 1));

    [Fact]
    public async Task ExecuteAsync_WhenOverdraftExists_ShouldReturnDetails()
    {
        var overdraft = ValidOverdraftContract();
        _repository.Setup(r => r.GetOverdraftByIdAsync(overdraft.Id, default))
            .ReturnsAsync(overdraft);

        var result = await _sut.ExecuteAsync(overdraft.Id, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ContractNumber.Should().Be("OD-2024-001");
        result.Value.ContractAmount.Should().Be(5_000m);
        result.Value.OutstandingBalance.Should().Be(3_200m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOverdraftNotFound_ShouldReturnSuccessWithNull()
    {
        var missingId = Guid.NewGuid();
        _repository.Setup(r => r.GetOverdraftByIdAsync(missingId, default))
            .ReturnsAsync((OverdraftContract?)null);

        var result = await _sut.ExecuteAsync(missingId, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenOverdraftExists_ShouldReturnActiveStatus()
    {
        var overdraft = ValidOverdraftContract();
        _repository.Setup(r => r.GetOverdraftByIdAsync(overdraft.Id, default))
            .ReturnsAsync(overdraft);

        var result = await _sut.ExecuteAsync(overdraft.Id, Guid.NewGuid());

        result.Value!.Status.Should().Be(ContractStatus.Active);
    }
}
