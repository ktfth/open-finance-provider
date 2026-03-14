using FluentAssertions;
using Moq;
using OpenFinance.InvestmentService.Application.UseCases;
using OpenFinance.InvestmentService.Domain.Entities;
using OpenFinance.InvestmentService.Domain.Repositories;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.InvestmentService.Tests.Application;

public class GetFixedIncomeTransactionsUseCaseTests
{
    private readonly Mock<IInvestmentRepository> _repository = new();
    private readonly GetFixedIncomeTransactionsUseCase _sut;

    public GetFixedIncomeTransactionsUseCaseTests() =>
        _sut = new GetFixedIncomeTransactionsUseCase(_repository.Object);

    private static FixedIncomeInvestment ValidCDB() =>
        FixedIncomeInvestment.Create(
            "user-1", FixedIncomeType.CDB, "CDB 120% CDI", "Bank", "ISIN001",
            new DateOnly(2024, 1, 1), new DateOnly(2025, 12, 31),
            1000m, 1000m, 5m, "BRL",
            RateIndexer.CDI, 120m, 0m, 0m, 0m, RemunType.PostFixed);

    [Fact]
    public async Task ExecuteAsync_WithValidRange_ShouldReturnTransactions()
    {
        var investment = ValidCDB();
        var transactions = new List<FixedIncomeTransaction>
        {
            FixedIncomeTransaction.Create(investment.Id, FixedIncomeTransactionType.Purchase,
                new DateOnly(2024, 1, 15), 5m, 1000m, 5000m, 0m, "BRL")
        };

        _repository.Setup(r => r.GetFixedIncomeByIdAsync(investment.Id, default)).ReturnsAsync(investment);
        _repository.Setup(r => r.GetFixedIncomeTransactionsAsync(
            investment.Id, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31), default))
            .ReturnsAsync(transactions);

        var result = await _sut.ExecuteAsync(
            investment.Id, Guid.NewGuid(),
            new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31));

        result.IsSuccess.Should().BeTrue();
        result.Value.Transactions.Should().HaveCount(1);
        result.Value.Transactions[0].Type.Should().Be(FixedIncomeTransactionType.Purchase);
        result.Value.Transactions[0].NetValue.Should().Be(5000m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenFromAfterTo_ShouldReturnFailure()
    {
        var result = await _sut.ExecuteAsync(
            Guid.NewGuid(), Guid.NewGuid(),
            new DateOnly(2024, 12, 31), new DateOnly(2024, 1, 1));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("from");
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvestmentNotFound_ShouldReturnFailure()
    {
        _repository.Setup(r => r.GetFixedIncomeByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((FixedIncomeInvestment?)null);

        var result = await _sut.ExecuteAsync(
            Guid.NewGuid(), Guid.NewGuid(),
            new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }
}
