using FluentAssertions;
using Moq;
using OpenFinance.ConsentService.Application.UseCases;
using OpenFinance.ConsentService.Domain.Entities;
using OpenFinance.ConsentService.Domain.Repositories;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.ConsentService.Tests.Application;

public class RevokeConsentUseCaseTests
{
    private readonly Mock<IConsentRepository> _repository = new();
    private readonly RevokeConsentUseCase _sut;

    public RevokeConsentUseCaseTests() =>
        _sut = new RevokeConsentUseCase(_repository.Object);

    private static Consent AuthorisedConsent()
    {
        var c = Consent.Create("c", "u", ["P"], DateTime.UtcNow.AddDays(1));
        c.Authorise();
        return c;
    }

    [Fact]
    public async Task ExecuteAsync_WithValidConsent_ShouldRevokeAndReturnSuccess()
    {
        var consent = AuthorisedConsent();
        _repository.Setup(r => r.GetByIdAsync(consent.Id, default)).ReturnsAsync(consent);
        _repository.Setup(r => r.UpdateAsync(consent, default)).Returns(Task.CompletedTask);

        var result = await _sut.ExecuteAsync(consent.Id, "Customer request");

        result.IsSuccess.Should().BeTrue();
        consent.Status.Should().Be(ConsentStatus.Revoked);
        _repository.Verify(r => r.UpdateAsync(consent, default), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenConsentNotFound_ShouldReturnFailure()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Consent?)null);

        var result = await _sut.ExecuteAsync(Guid.NewGuid(), "reason");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlreadyRevoked_ShouldReturnFailure()
    {
        var consent = AuthorisedConsent();
        consent.Revoke("first revoke");
        _repository.Setup(r => r.GetByIdAsync(consent.Id, default)).ReturnsAsync(consent);

        var result = await _sut.ExecuteAsync(consent.Id, "second revoke");

        result.IsFailure.Should().BeTrue();
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Consent>(), default), Times.Never);
    }
}
