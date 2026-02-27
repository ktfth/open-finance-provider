using FluentAssertions;
using Moq;
using OpenFinance.ConsentService.Application.UseCases;
using OpenFinance.ConsentService.Domain.Entities;
using OpenFinance.ConsentService.Domain.Repositories;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.ConsentService.Tests.Application;

public class CreateConsentUseCaseTests
{
    private readonly Mock<IConsentRepository> _repository = new();
    private readonly CreateConsentUseCase _sut;

    public CreateConsentUseCaseTests() =>
        _sut = new CreateConsentUseCase(_repository.Object);

    private static CreateConsentRequest ValidRequest() => new(
        ClientId: "bank-001",
        UserId: "user-abc",
        Permissions: ["ACCOUNTS_READ"],
        ExpiresAt: DateTime.UtcNow.AddDays(30));

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldReturnSuccess()
    {
        _repository.Setup(r => r.AddAsync(It.IsAny<Consent>(), default)).Returns(Task.CompletedTask);

        var result = await _sut.ExecuteAsync(ValidRequest());

        result.IsSuccess.Should().BeTrue();
        result.Value.ClientId.Should().Be("bank-001");
        result.Value.UserId.Should().Be("user-abc");
        result.Value.Status.Should().Be(ConsentStatus.Pending);
        _repository.Verify(r => r.AddAsync(It.IsAny<Consent>(), default), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyClientId_ShouldReturnFailure()
    {
        var request = ValidRequest() with { ClientId = "" };
        var result = await _sut.ExecuteAsync(request);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("ClientId");
        _repository.Verify(r => r.AddAsync(It.IsAny<Consent>(), default), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithPastExpiry_ShouldReturnFailure()
    {
        var request = ValidRequest() with { ExpiresAt = DateTime.UtcNow.AddDays(-1) };
        var result = await _sut.ExecuteAsync(request);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPersistConsent()
    {
        Consent? captured = null;
        _repository.Setup(r => r.AddAsync(It.IsAny<Consent>(), default))
            .Callback<Consent, CancellationToken>((c, _) => captured = c)
            .Returns(Task.CompletedTask);

        await _sut.ExecuteAsync(ValidRequest());

        captured.Should().NotBeNull();
        captured!.ClientId.Should().Be("bank-001");
        captured.Status.Should().Be(ConsentStatus.Pending);
    }
}
