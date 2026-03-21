using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OpenFinance.Shared.Infrastructure;

/// <summary>
/// Logs every request with correlation ID, duration, consent ID, and response status.
/// Critical for Open Finance audit trail requirements.
/// </summary>
public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.TraceIdentifier;
        var consentId = context.Request.Headers["x-consent-id"].FirstOrDefault() ?? "none";
        var stopwatch = Stopwatch.StartNew();

        // Add correlation ID to response headers for traceability
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["x-correlation-id"] = correlationId;
            return Task.CompletedTask;
        });

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms. ConsentId={ConsentId} CorrelationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                consentId,
                correlationId);
        }
    }
}
