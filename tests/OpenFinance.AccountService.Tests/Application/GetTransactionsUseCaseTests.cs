using FluentAssertions;
using Moq;
using OpenFinance.AccountService.Application.UseCases;
using OpenFinance.AccountService.Domain.Entities;
using OpenFinance.AccountService.Domain.Repositories;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.AccountService.Tests.Application;

public class GetTransactionsUseCaseTests
{
    private readonly Mock<IAccountRepository> _repository = new();
    private readonly GetTransactionsUseCase _sut;

    public GetTransactionsUseCaseTests() =>
        _sut = new GetTransactionsUseCase(_repository.Object);

    private static Account ActiveAccount()
    {
        var a = Account.Create("user-1", "ACC001", "0001", AccountType.Checking, "BRL", "John", "12345678901");
        return a;
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRange_ShouldReturnTransactions()
    {
        var account = ActiveAccount();
        var consentId = Guid.NewGuid();
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var to = DateOnly.FromDateTime(DateTime.UtcNow);

        var transactions = new List<Transaction>
        {
            Transaction.Create(account.Id, 100m, "BRL", "CREDIT", "Salary", DateTime.UtcNow.AddDays(-3)),
            Transaction.Create(account.Id, -50m, "BRL", "DEBIT", "Market", DateTime.UtcNow.AddDays(-1))
        };

        _repository.Setup(r => r.GetByIdAsync(account.Id, default)).ReturnsAsync(account);
        _repository.Setup(r => r.GetTransactionsAsync(account.Id, from, to, default))
            .ReturnsAsync(transactions);

        var result = await _sut.ExecuteAsync(account.Id, consentId, from, to);

        result.IsSuccess.Should().BeTrue();
        result.Value.Transactions.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_WithFromAfterTo_ShouldReturnFailure()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);
        var to = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        var result = await _sut.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), from, to);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("'from'");
    }

    [Fact]
    public async Task ExecuteAsync_WhenAccountNotFound_ShouldReturnFailure()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Account?)null);

        var result = await _sut.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }
}
