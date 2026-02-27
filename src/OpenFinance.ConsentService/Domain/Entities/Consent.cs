using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Domain;

namespace OpenFinance.ConsentService.Domain.Entities;

public class Consent : Entity
{
    private List<string> _permissions = [];

    private Consent() { }

    public static Consent Create(
        string clientId,
        string userId,
        IEnumerable<string> permissions,
        DateTime expiresAt,
        string? redirectUri = null)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("ClientId is required.", nameof(clientId));
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));
        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("ExpiresAt must be in the future.", nameof(expiresAt));

        var permList = permissions?.ToList() ?? [];
        if (permList.Count == 0)
            throw new ArgumentException("At least one permission is required.", nameof(permissions));

        var consent = new Consent
        {
            ClientId = clientId,
            UserId = userId,
            ExpiresAt = expiresAt,
            RedirectUri = redirectUri,
            Status = ConsentStatus.Pending
        };
        consent._permissions.AddRange(permList);
        return consent;
    }

    public string ClientId { get; private set; } = default!;
    public string UserId { get; private set; } = default!;
    public IReadOnlyList<string> Permissions => _permissions.AsReadOnly();
    public ConsentStatus Status { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public string? RedirectUri { get; private set; }
    public string? RejectionReason { get; private set; }

    public void Authorise()
    {
        if (Status != ConsentStatus.Pending)
            throw new InvalidOperationException($"Cannot authorise consent in status {Status}.");
        Status = ConsentStatus.Authorised;
        SetUpdated();
    }

    public void Reject(string reason)
    {
        if (Status != ConsentStatus.Pending)
            throw new InvalidOperationException($"Cannot reject consent in status {Status}.");
        Status = ConsentStatus.Rejected;
        RejectionReason = reason;
        SetUpdated();
    }

    public void Revoke(string reason)
    {
        if (Status is ConsentStatus.Revoked or ConsentStatus.Expired)
            throw new InvalidOperationException($"Cannot revoke consent in status {Status}.");
        Status = ConsentStatus.Revoked;
        RejectionReason = reason;
        SetUpdated();
    }

    public bool IsActive() =>
        Status == ConsentStatus.Authorised && ExpiresAt > DateTime.UtcNow;

    public bool HasPermissions(IEnumerable<string> required) =>
        required.All(p => _permissions.Contains(p, StringComparer.OrdinalIgnoreCase));
}
