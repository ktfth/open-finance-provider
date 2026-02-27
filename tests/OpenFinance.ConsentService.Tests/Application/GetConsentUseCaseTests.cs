using FluentAssertions;
using Moq;
using OpenFinance.ConsentService.Application.UseCases;
using OpenFinance.ConsentService.Domain.Entities;
using OpenFinance.ConsentService.Domain.Repositories;

namespace OpenFinance.ConsentService.Tests.Application;

public class GetConsentUseCaseTests
{
    private readonly Mock<IConsentRepository> _repository = new();
    private readonly GetConsentUseCase _sut;

    public GetConsentUseCaseTests() =>
        _sut = new GetConsentUseCase(_repository.Object);

    [Fact]
    public async Task ExecuteAsync_WhenConsentExists_ShouldReturnMappedResponse()
    {
        var consent = Consent.Create("bank-1", "user-1", ["ACCOUNTS_READ"], DateTime.UtcNow.AddDays(1));
        _repository.Setup(r => r.GetByIdAsync(consent.Id, default)).ReturnsAsync(consent);

        var result = await _sut.ExecuteAsync(consent.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ClientId.Should().Be("bank-1");
        result.Value.ConsentId.Should().Be(consent.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WhenConsentNotFound_ShouldReturnNullValue()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Consent?)null);

        var result = await _sut.ExecuteAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }
}
