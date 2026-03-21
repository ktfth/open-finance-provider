using FluentAssertions;
using Moq;
using OpenFinance.ResourcesService.Application.UseCases;
using OpenFinance.ResourcesService.Domain.Repositories;
using OpenFinance.Shared.Consent;

namespace OpenFinance.ResourcesService.Tests.Application;

public class ConsentValidationTests
{
    private readonly Mock<IResourceRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();

    [Fact]
    public async Task GetResources_WithInvalidConsent_ReturnsFailure()
    {
        var consentId = Guid.NewGuid();

        _consentValidator.Setup(v => v.ValidateAsync(
            It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.InvalidConsent("Consent not found"));

        var useCase = new GetResourcesUseCase(_repository.Object, _consentValidator.Object);
        var result = await useCase.ExecuteAsync(consentId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Consent not found");
        _repository.Verify(r => r.GetByConsentIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetResources_WithInsufficientPermissions_ReturnsFailure()
    {
        var consentId = Guid.NewGuid();

        _consentValidator.Setup(v => v.ValidateAsync(
            It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.InsufficientPermissions("Missing RESOURCES_READ permission"));

        var useCase = new GetResourcesUseCase(_repository.Object, _consentValidator.Object);
        var result = await useCase.ExecuteAsync(consentId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Missing");
        _repository.Verify(r => r.GetByConsentIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetResources_WithServiceUnavailable_ReturnsFailure()
    {
        var consentId = Guid.NewGuid();

        _consentValidator.Setup(v => v.ValidateAsync(
            It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.ServiceUnavailable("Consent service is unavailable"));

        var useCase = new GetResourcesUseCase(_repository.Object, _consentValidator.Object);
        var result = await useCase.ExecuteAsync(consentId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("unavailable");
        _repository.Verify(r => r.GetByConsentIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
