namespace OpenFinance.Shared.Contracts;

/// <summary>
/// Defines the contract for consent management operations provided to bank participants.
/// </summary>
public interface IConsentContract
{
    Task<ConsentResponse> CreateConsentAsync(CreateConsentRequest request, CancellationToken ct = default);
    Task<ConsentResponse?> GetConsentAsync(Guid consentId, CancellationToken ct = default);
    Task<bool> RevokeConsentAsync(Guid consentId, string reason, CancellationToken ct = default);
    Task<bool> IsConsentActiveAsync(Guid consentId, string[] requiredPermissions, CancellationToken ct = default);
}

public record CreateConsentRequest(
    string ClientId,
    string UserId,
    string[] Permissions,
    DateTime ExpiresAt,
    string? RedirectUri = null
);

public record ConsentResponse(
    Guid ConsentId,
    string ClientId,
    string UserId,
    string[] Permissions,
    ConsentStatus Status,
    DateTime CreatedAt,
    DateTime ExpiresAt
);

public enum ConsentStatus
{
    Pending,
    Authorised,
    Rejected,
    Revoked,
    Expired
}
