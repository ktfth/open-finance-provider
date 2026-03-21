using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OpenFinance.Shared.Infrastructure;

/// <summary>
/// Extension methods for converting Result failures to standardized ProblemDetails responses.
/// Eliminates error-handling duplication across all controllers.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result error string to the appropriate IActionResult with ProblemDetails.
    /// Handles consent errors (403/503), not-found (404), and general bad requests (400).
    /// </summary>
    public static IActionResult ToErrorResponse(this string error)
    {
        if (error.Contains("CONSENT_SERVICE_UNAVAILABLE"))
            return new ObjectResult(OpenFinanceProblemDetails.ServiceUnavailable(error))
                { StatusCode = StatusCodes.Status503ServiceUnavailable };

        if (error.Contains("CONSENT_"))
            return new ObjectResult(OpenFinanceProblemDetails.Forbidden(error))
                { StatusCode = StatusCodes.Status403Forbidden };

        if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return new ObjectResult(OpenFinanceProblemDetails.NotFound(error))
                { StatusCode = StatusCodes.Status404NotFound };

        return new ObjectResult(OpenFinanceProblemDetails.BadRequest(error))
            { StatusCode = StatusCodes.Status400BadRequest };
    }
}
