using FluentAssertions;
using Moq;
using OpenFinance.AccountService.Application.UseCases;
using OpenFinance.AccountService.Domain.Repositories;
using OpenFinance.Shared.Consent;

namespace OpenFinance.AccountService.Tests.Application;

public class ConsentValidationTests
{
    private readonly Mock<IAccountRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();

    [Fact]
    public async Task GetAccounts_WithInvalidConsent_ReturnsFailure()
    {
        _consentValidator.Setup(v => v.ValidateAsync(
            It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.InvalidConsent("Consent not found"));

        var useCase = new GetAccountsUseCase(_repository.Object, _consentValidator.Object);
        var result = await useCase.ExecuteAsync("user-123", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Consent not found");
        _repository.Verify(r => r.GetByUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAccounts_WithInsufficientPermissions_ReturnsFailure()
    {
        _consentValidator.Setup(v => v.ValidateAsync(
            It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.InsufficientPermissions("Missing ACCOUNTS_READ permission"));

        var useCase = new GetAccountsUseCase(_repository.Object, _consentValidator.Object);
        var result = await useCase.ExecuteAsync("user-123", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Missing");
        _repository.Verify(r => r.GetByUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAccounts_WithServiceUnavailable_ReturnsFailure()
    {
        _consentValidator.Setup(v => v.ValidateAsync(
            It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.ServiceUnavailable("Consent service is unavailable"));

        var useCase = new GetAccountsUseCase(_repository.Object, _consentValidator.Object);
        var result = await useCase.ExecuteAsync("user-123", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("unavailable");
        _repository.Verify(r => r.GetByUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
