namespace OpenFinance.Shared.Consent;

/// <summary>
/// Validates consent status and permissions via the ConsentService.
/// Each microservice injects this to verify the x-consent-id header
/// before returning protected data.
/// </summary>
public interface IConsentValidator
{
    /// <summary>
    /// Validates that a consent is active and has the required permissions.
    /// Returns a ConsentValidationResult indicating success or the reason for failure.
    /// </summary>
    Task<ConsentValidationResult> ValidateAsync(
        Guid consentId,
        string[] requiredPermissions,
        CancellationToken ct = default);
}

public sealed record ConsentValidationResult
{
    public bool IsValid { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static ConsentValidationResult Valid() => new() { IsValid = true };

    public static ConsentValidationResult InvalidConsent(string message) => new()
    {
        IsValid = false,
        ErrorCode = "CONSENT_INVALID",
        ErrorMessage = message
    };

    public static ConsentValidationResult InsufficientPermissions(string message) => new()
    {
        IsValid = false,
        ErrorCode = "CONSENT_INSUFFICIENT_PERMISSIONS",
        ErrorMessage = message
    };

    public static ConsentValidationResult ServiceUnavailable(string message) => new()
    {
        IsValid = false,
        ErrorCode = "CONSENT_SERVICE_UNAVAILABLE",
        ErrorMessage = message
    };
}
