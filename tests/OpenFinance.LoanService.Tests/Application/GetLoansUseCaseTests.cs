using FluentAssertions;
using Moq;
using OpenFinance.LoanService.Application.UseCases;
using OpenFinance.LoanService.Domain.Entities;
using OpenFinance.LoanService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.LoanService.Tests.Application;

public class GetLoansUseCaseTests
{
    private readonly Mock<ILoanRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetLoansUseCase _sut;

    public GetLoansUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetLoansUseCase(_repository.Object, _consentValidator.Object);
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
    public async Task ExecuteAsync_WhenUserHasLoans_ShouldReturnMappedSummaries()
    {
        var loan = ValidLoanContract();
        _repository.Setup(r => r.GetLoansByUserIdAsync("user-001", default))
            .ReturnsAsync(new[] { loan });

        var result = await _sut.ExecuteAsync("user-001", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Loans.Should().HaveCount(1);
        result.Value.Loans[0].ContractNumber.Should().Be("LOAN-2024-001");
        result.Value.Loans[0].Type.Should().Be(LoanType.PersonalLoan);
        result.Value.Loans[0].ContractAmount.Should().Be(10_000m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserHasNoLoans_ShouldReturnEmptyList()
    {
        _repository.Setup(r => r.GetLoansByUserIdAsync("user-002", default))
            .ReturnsAsync(Array.Empty<LoanContract>());

        var result = await _sut.ExecuteAsync("user-002", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Loans.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserHasMultipleLoans_ShouldReturnAllMapped()
    {
        var loan1 = ValidLoanContract();
        var loan2 = LoanContract.Create(
            userId: "user-001",
            contractNumber: "LOAN-2024-002",
            type: LoanType.PayrollLoan,
            productName: "Empréstimo Consignado",
            companyCnpj: "12.345.678/0001-99",
            contractAmount: 20_000m,
            outstandingBalance: 18_000m,
            interestRate: 0.014m,
            interestRateType: InterestRateType.Simple,
            indexer: RateIndexer.PreFixed,
            currency: "BRL",
            contractDate: new DateOnly(2024, 3, 1),
            dueDate: new DateOnly(2027, 3, 1),
            settlementDate: new DateOnly(2027, 3, 1),
            instalmentCount: 36,
            paidInstalmentCount: 3,
            cet: 0.018m,
            amortizationType: AmortizationType.SAC);

        _repository.Setup(r => r.GetLoansByUserIdAsync("user-001", default))
            .ReturnsAsync(new[] { loan1, loan2 });

        var result = await _sut.ExecuteAsync("user-001", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Loans.Should().HaveCount(2);
    }
}
