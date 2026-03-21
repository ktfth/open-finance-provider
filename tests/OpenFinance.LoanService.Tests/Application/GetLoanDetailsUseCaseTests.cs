using FluentAssertions;
using Moq;
using OpenFinance.LoanService.Application.UseCases;
using OpenFinance.LoanService.Domain.Entities;
using OpenFinance.LoanService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.LoanService.Tests.Application;

public class GetLoanDetailsUseCaseTests
{
    private readonly Mock<ILoanRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetLoanDetailsUseCase _sut;

    public GetLoanDetailsUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetLoanDetailsUseCase(_repository.Object, _consentValidator.Object);
    }

    private static LoanContract ValidLoanContract() =>
        LoanContract.Create(
            userId: "user-001",
            contractNumber: "LOAN-2024-001",
            type: LoanType.PersonalLoan,
            productName: "Crédito Pessoal",
            companyCnpj: "12.345.678/0001-99",
            contractAmount: 10_000m,
            outstandingBalance: 8_500m,
            interestRate: 0.0199m,
            interestRateType: InterestRateType.Compound,
            indexer: RateIndexer.CDI,
            currency: "BRL",
            contractDate: new DateOnly(2024, 1, 10),
            dueDate: new DateOnly(2026, 1, 10),
            settlementDate: new DateOnly(2026, 1, 10),
            instalmentCount: 24,
            paidInstalmentCount: 6,
            cet: 0.025m,
            amortizationType: AmortizationType.Price);

    [Fact]
    public async Task ExecuteAsync_WhenLoanExists_ShouldReturnDetailedResponse()
    {
        var loan = ValidLoanContract();
        var contractId = loan.Id;
        _repository.Setup(r => r.GetLoanByIdAsync(contractId, default))
            .ReturnsAsync(loan);

        var result = await _sut.ExecuteAsync(contractId, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ContractNumber.Should().Be("LOAN-2024-001");
        result.Value.OutstandingBalance.Should().Be(8_500m);
        result.Value.InstalmentCount.Should().Be(24);
        result.Value.PaidInstalmentCount.Should().Be(6);
        result.Value.AmortizationType.Should().Be(AmortizationType.Price);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoanNotFound_ShouldReturnSuccessWithNull()
    {
        var missingId = Guid.NewGuid();
        _repository.Setup(r => r.GetLoanByIdAsync(missingId, default))
            .ReturnsAsync((LoanContract?)null);

        var result = await _sut.ExecuteAsync(missingId, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoanExists_ShouldReturnEmptyFeesCollection()
    {
        var loan = ValidLoanContract();
        _repository.Setup(r => r.GetLoanByIdAsync(loan.Id, default))
            .ReturnsAsync(loan);

        var result = await _sut.ExecuteAsync(loan.Id, Guid.NewGuid());

        result.Value!.Fees.Should().BeEmpty();
    }
}
