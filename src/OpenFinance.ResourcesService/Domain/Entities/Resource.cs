using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.ResourcesService.Domain.Entities;

public class Resource : Entity
{
    private Resource() { }

    public static Resource Create(
        Guid consentId,
        Guid resourceId,
        ResourceType type,
        ResourceStatus status = ResourceStatus.Available)
    {
        return new Resource
        {
            ConsentId = consentId,
            ResourceId = resourceId,
            Type = type,
            Status = status
        };
    }

    public Guid ConsentId { get; private set; }
    public Guid ResourceId { get; private set; }
    public ResourceType Type { get; private set; }
    public ResourceStatus Status { get; private set; }

    public void MarkUnavailable()
    {
        Status = ResourceStatus.Unavailable;
        SetUpdated();
    }

    public void MarkAvailable()
    {
        Status = ResourceStatus.Available;
        SetUpdated();
    }
}
