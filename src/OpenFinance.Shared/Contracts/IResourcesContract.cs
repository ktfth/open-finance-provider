namespace OpenFinance.Shared.Contracts;

/// <summary>
/// Defines the contract for the Resources Discovery API.
/// Lists all resources available for a given consent (Phase 2/3).
/// Required by the Open Finance Brasil specification for resource discovery.
/// </summary>
public interface IResourcesContract
{
    Task<ResourceListResponse> GetResourcesAsync(Guid consentId, CancellationToken ct = default);
}

public record ResourceListResponse(IReadOnlyList<ResourceSummary> Resources);

public record ResourceSummary(
    Guid ResourceId,
    ResourceType Type,
    ResourceStatus Status
);

public enum ResourceType
{
    Account,
    CreditCard,
    Loan,
    Financing,
    UnarrangedOverdraft,
    FixedIncome,
    VariableIncome,
    TreasuryBond,
    Fund,
    Insurance,
    Pension,
    CapitalizationBond,
    Exchange
}

public enum ResourceStatus
{
    Available,
    Unavailable,
    TemporarilyUnavailable,
    PendingAuthorisation
}
