using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OpenFinance.Shared.Infrastructure;

/// <summary>
/// Global exception handler that returns RFC 7807 ProblemDetails for all unhandled exceptions.
/// Prevents stack traces from leaking to clients.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.TraceIdentifier;

        logger.LogError(exception,
            "Unhandled exception. CorrelationId={CorrelationId}, Path={Path}, Method={Method}",
            correlationId, httpContext.Request.Path, httpContext.Request.Method);

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred. Please contact support with the correlation ID.",
            Type = "https://openfinancebrasil.atlassian.net/wiki/errors/internal-server-error",
            Extensions =
            {
                ["correlationId"] = correlationId
            }
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
