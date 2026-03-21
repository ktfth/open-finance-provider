using FluentAssertions;
using OpenFinance.LoanService.Domain.Entities;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.LoanService.Tests.Domain;

public class LoanContractTests
{
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
    public void Create_WithValidData_ShouldCreateActiveContract()
    {
        var loan = ValidLoanContract();

        loan.Status.Should().Be(ContractStatus.Active);
        loan.ContractNumber.Should().Be("LOAN-2024-001");
        loan.ContractAmount.Should().Be(10_000m);
        loan.InstalmentCount.Should().Be(24);
    }

    [Fact]
    public void Settle_WhenActive_ShouldSetStatusAndZeroBalance()
    {
        var loan = ValidLoanContract();
        loan.Settle();

        loan.Status.Should().Be(ContractStatus.Settled);
        loan.OutstandingBalance.Should().Be(0m);
    }

    [Fact]
    public void WriteOff_WhenActive_ShouldSetWrittenOffStatus()
    {
        var loan = ValidLoanContract();
        loan.WriteOff();

        loan.Status.Should().Be(ContractStatus.WrittenOff);
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrow()
    {
        var act = () => LoanContract.Create(
            "", "LOAN-001", LoanType.PersonalLoan, "Product", "12.345.678/0001-99",
            10_000m, 8_500m, 0.02m, InterestRateType.Compound, RateIndexer.CDI, "BRL",
            new DateOnly(2024, 1, 1), new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 1),
            24, 0, 0.025m, AmortizationType.Price);

        act.Should().Throw<ArgumentException>();
    }
}
