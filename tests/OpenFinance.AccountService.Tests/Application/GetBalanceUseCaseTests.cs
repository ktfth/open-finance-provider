using FluentAssertions;
using Moq;
using OpenFinance.AccountService.Application.UseCases;
using OpenFinance.AccountService.Domain.Entities;
using OpenFinance.AccountService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.AccountService.Tests.Application;

public class GetBalanceUseCaseTests
{
    private readonly Mock<IAccountRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetBalanceUseCase _sut;

    public GetBalanceUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetBalanceUseCase(_repository.Object, _consentValidator.Object);
    }

    private static Account ActiveAccount() =>
        Account.Create("user-1", "ACC001", "0001", AccountType.Checking, "BRL", "John", "12345678901");

    [Fact]
    public async Task ExecuteAsync_WithValidAccountAndBalance_ShouldReturnBalance()
    {
        var account = ActiveAccount();
        var balance = AccountBalance.Create(account.Id, 1500m, 200m, "BRL");
        var consentId = Guid.NewGuid();

        _repository.Setup(r => r.GetByIdAsync(account.Id, default)).ReturnsAsync(account);
        _repository.Setup(r => r.GetBalanceAsync(account.Id, default)).ReturnsAsync(balance);

        var result = await _sut.ExecuteAsync(account.Id, consentId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AvailableBalance.Should().Be(1500m);
        result.Value.BlockedBalance.Should().Be(200m);
        result.Value.Currency.Should().Be("BRL");
    }

    [Fact]
    public async Task ExecuteAsync_WhenAccountNotFound_ShouldReturnNull()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Account?)null);

        var result = await _sut.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenAccountInactive_ShouldReturnNull()
    {
        var account = ActiveAccount();
        account.Deactivate();
        _repository.Setup(r => r.GetByIdAsync(account.Id, default)).ReturnsAsync(account);

        var result = await _sut.ExecuteAsync(account.Id, Guid.NewGuid());

        result.Value.Should().BeNull();
        _repository.Verify(r => r.GetBalanceAsync(It.IsAny<Guid>(), default), Times.Never);
    }
}
