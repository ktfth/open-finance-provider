using FluentAssertions;
using Moq;
using OpenFinance.ConsentService.Application.UseCases;
using OpenFinance.ConsentService.Domain.Entities;
using OpenFinance.ConsentService.Domain.Repositories;

namespace OpenFinance.ConsentService.Tests.Application;

public class ValidateConsentUseCaseTests
{
    private readonly Mock<IConsentRepository> _repository = new();
    private readonly ValidateConsentUseCase _sut;

    public ValidateConsentUseCaseTests() =>
        _sut = new ValidateConsentUseCase(_repository.Object);

    private static Consent AuthorisedConsent(string[] permissions)
    {
        var c = Consent.Create("c", "u", permissions, DateTime.UtcNow.AddDays(1));
        c.Authorise();
        return c;
    }

    [Fact]
    public async Task ExecuteAsync_WithActiveConsentAndMatchingPermissions_ShouldReturnTrue()
    {
        var consent = AuthorisedConsent(["ACCOUNTS_READ", "TRANSACTIONS_READ"]);
        _repository.Setup(r => r.GetByIdAsync(consent.Id, default)).ReturnsAsync(consent);

        var result = await _sut.ExecuteAsync(consent.Id, ["ACCOUNTS_READ"]);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingPermission_ShouldReturnFalse()
    {
        var consent = AuthorisedConsent(["ACCOUNTS_READ"]);
        _repository.Setup(r => r.GetByIdAsync(consent.Id, default)).ReturnsAsync(consent);

        var result = await _sut.ExecuteAsync(consent.Id, ["PAYMENTS_WRITE"]);

        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WithRevokedConsent_ShouldReturnFalse()
    {
        var consent = AuthorisedConsent(["ACCOUNTS_READ"]);
        consent.Revoke("test");
        _repository.Setup(r => r.GetByIdAsync(consent.Id, default)).ReturnsAsync(consent);

        var result = await _sut.ExecuteAsync(consent.Id, ["ACCOUNTS_READ"]);

        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WhenConsentNotFound_ShouldReturnFalse()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Consent?)null);

        var result = await _sut.ExecuteAsync(Guid.NewGuid(), ["ACCOUNTS_READ"]);

        result.Value.Should().BeFalse();
    }
}
