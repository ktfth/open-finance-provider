using FluentAssertions;
using Moq;
using OpenFinance.ResourcesService.Application.UseCases;
using OpenFinance.ResourcesService.Domain.Entities;
using OpenFinance.ResourcesService.Domain.Repositories;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.ResourcesService.Tests.Application;

public class GetResourcesUseCaseTests
{
    private readonly Mock<IResourceRepository> _repository = new();
    private readonly Mock<IConsentValidator> _consentValidator = new();
    private readonly GetResourcesUseCase _sut;

    public GetResourcesUseCaseTests()
    {
        _consentValidator
            .Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentValidationResult.Valid());
        _sut = new GetResourcesUseCase(_repository.Object, _consentValidator.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenConsentHasResources_ShouldReturnMappedSummaries()
    {
        var consentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var loanId = Guid.NewGuid();

        var resources = new[]
        {
            Resource.Create(consentId, accountId, ResourceType.Account, ResourceStatus.Available),
            Resource.Create(consentId, loanId, ResourceType.Loan, ResourceStatus.Available)
        };

        _repository.Setup(r => r.GetByConsentIdAsync(consentId, default))
            .ReturnsAsync(resources);

        var result = await _sut.ExecuteAsync(consentId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Resources.Should().HaveCount(2);
        result.Value.Resources.Should().Contain(r => r.Type == ResourceType.Account);
        result.Value.Resources.Should().Contain(r => r.Type == ResourceType.Loan);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoResourcesExistForConsent_ShouldReturnEmptyList()
    {
        var consentId = Guid.NewGuid();

        _repository.Setup(r => r.GetByConsentIdAsync(consentId, default))
            .ReturnsAsync(Array.Empty<Resource>());

        var result = await _sut.ExecuteAsync(consentId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Resources.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenResourceIsUnavailable_ShouldMapStatusCorrectly()
    {
        var consentId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();

        var resource = Resource.Create(consentId, resourceId, ResourceType.CreditCard, ResourceStatus.Available);
        resource.MarkUnavailable();

        _repository.Setup(r => r.GetByConsentIdAsync(consentId, default))
            .ReturnsAsync(new[] { resource });

        var result = await _sut.ExecuteAsync(consentId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Resources[0].Status.Should().Be(ResourceStatus.Unavailable);
        result.Value.Resources[0].Type.Should().Be(ResourceType.CreditCard);
    }
}
