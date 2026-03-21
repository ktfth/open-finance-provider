using FluentAssertions;
using OpenFinance.ResourcesService.Domain.Entities;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.ResourcesService.Tests.Domain;

public class ResourceTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateAvailableResource()
    {
        var consentId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();

        var resource = Resource.Create(consentId, resourceId, ResourceType.Account);

        resource.ConsentId.Should().Be(consentId);
        resource.ResourceId.Should().Be(resourceId);
        resource.Type.Should().Be(ResourceType.Account);
        resource.Status.Should().Be(ResourceStatus.Available);
    }

    [Fact]
    public void MarkUnavailable_WhenAvailable_ShouldChangeStatus()
    {
        var resource = Resource.Create(Guid.NewGuid(), Guid.NewGuid(), ResourceType.Loan);

        resource.MarkUnavailable();

        resource.Status.Should().Be(ResourceStatus.Unavailable);
    }

    [Fact]
    public void MarkAvailable_WhenUnavailable_ShouldRestoreStatus()
    {
        var resource = Resource.Create(Guid.NewGuid(), Guid.NewGuid(), ResourceType.Insurance, ResourceStatus.Unavailable);

        resource.MarkAvailable();

        resource.Status.Should().Be(ResourceStatus.Available);
    }
}
