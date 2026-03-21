using System.Net.Http.Json;

namespace OpenFinance.Shared.Consent;

/// <summary>
/// Validates consent by calling the ConsentService HTTP API.
/// GET {baseUrl}/open-finance/v1/consents/{id}/validate?permissions=X&permissions=Y
/// </summary>
public sealed class HttpConsentValidator(HttpClient httpClient) : IConsentValidator
{
    public async Task<ConsentValidationResult> ValidateAsync(
        Guid consentId,
        string[] requiredPermissions,
        CancellationToken ct = default)
    {
        if (consentId == Guid.Empty)
            return ConsentValidationResult.InvalidConsent("Consent ID is required.");

        try
        {
            var queryParams = string.Join("&", requiredPermissions.Select(p => $"permissions={Uri.EscapeDataString(p)}"));
            var url = $"open-finance/v1/consents/{consentId}/validate?{queryParams}";

            var response = await httpClient.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
                return ConsentValidationResult.InvalidConsent($"Consent {consentId} not found or service error.");

            var result = await response.Content.ReadFromJsonAsync<ConsentValidateResponse>(ct);
            if (result is null || !result.IsValid)
                return ConsentValidationResult.InsufficientPermissions(
                    $"Consent {consentId} is not active or lacks required permissions: {string.Join(", ", requiredPermissions)}");

            return ConsentValidationResult.Valid();
        }
        catch (HttpRequestException)
        {
            return ConsentValidationResult.ServiceUnavailable("Consent service is unavailable.");
        }
        catch (TaskCanceledException)
        {
            return ConsentValidationResult.ServiceUnavailable("Consent validation timed out.");
        }
    }

    private sealed record ConsentValidateResponse(bool IsValid);
}
