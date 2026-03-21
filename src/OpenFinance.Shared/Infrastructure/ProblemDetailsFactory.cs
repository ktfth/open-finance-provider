using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OpenFinance.Shared.Infrastructure;

/// <summary>
/// Standard RFC 7807 ProblemDetails error responses for Open Finance APIs.
/// </summary>
public static class OpenFinanceProblemDetails
{
    public static ProblemDetails Forbidden(string detail) => new()
    {
        Status = StatusCodes.Status403Forbidden,
        Title = "Forbidden",
        Detail = detail,
        Type = "https://openfinancebrasil.atlassian.net/wiki/errors/forbidden"
    };

    public static ProblemDetails ServiceUnavailable(string detail) => new()
    {
        Status = StatusCodes.Status503ServiceUnavailable,
        Title = "Service Unavailable",
        Detail = detail,
        Type = "https://openfinancebrasil.atlassian.net/wiki/errors/service-unavailable"
    };

    public static ProblemDetails BadRequest(string detail) => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Bad Request",
        Detail = detail,
        Type = "https://openfinancebrasil.atlassian.net/wiki/errors/bad-request"
    };

    public static ProblemDetails NotFound(string detail) => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Not Found",
        Detail = detail,
        Type = "https://openfinancebrasil.atlassian.net/wiki/errors/not-found"
    };

    public static ProblemDetails UnprocessableEntity(string detail) => new()
    {
        Status = StatusCodes.Status422UnprocessableEntity,
        Title = "Unprocessable Entity",
        Detail = detail,
        Type = "https://openfinancebrasil.atlassian.net/wiki/errors/unprocessable-entity"
    };

    public static ProblemDetails TooManyRequests(string detail, int retryAfterSeconds) => new()
    {
        Status = StatusCodes.Status429TooManyRequests,
        Title = "Too Many Requests",
        Detail = detail,
        Type = "https://openfinancebrasil.atlassian.net/wiki/errors/too-many-requests",
        Extensions = { ["retryAfter"] = retryAfterSeconds }
    };
}
