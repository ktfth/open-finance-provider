using FluentAssertions;
using Moq;
using OpenFinance.CreditCardService.Application.UseCases;
using OpenFinance.CreditCardService.Domain.Repositories;
using OpenFinance.Shared.Consent;

namespace OpenFinance.CreditCardService.Tests.Application;

public class ConsentValidationTests
{
    private readonly Mock<ICardRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();

    [Fact]
    public async Task GetCardAccounts_WithInvalidConsent_ReturnsFailure()
    {
        _consentValidator.Setup(v => v.ValidateAsync(
            It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.InvalidConsent("Consent not found"));

        var useCase = new GetCardAccountsUseCase(_repository.Object, _consentValidator.Object);
        var result = await useCase.ExecuteAsync("user-123", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Consent not found");
        _repository.Verify(r => r.GetCardAccountsByUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetCardAccounts_WithInsufficientPermissions_ReturnsFailure()
    {
        _consentValidator.Setup(v => v.ValidateAsync(
            It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.InsufficientPermissions("Missing CREDIT_CARDS_ACCOUNTS_READ permission"));

        var useCase = new GetCardAccountsUseCase(_repository.Object, _consentValidator.Object);
        var result = await useCase.ExecuteAsync("user-123", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Missing");
        _repository.Verify(r => r.GetCardAccountsByUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetCardAccounts_WithServiceUnavailable_ReturnsFailure()
    {
        _consentValidator.Setup(v => v.ValidateAsync(
            It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.ServiceUnavailable("Consent service is unavailable"));

        var useCase = new GetCardAccountsUseCase(_repository.Object, _consentValidator.Object);
        var result = await useCase.ExecuteAsync("user-123", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("unavailable");
        _repository.Verify(r => r.GetCardAccountsByUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
