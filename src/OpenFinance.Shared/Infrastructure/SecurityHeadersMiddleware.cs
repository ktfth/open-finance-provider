using Microsoft.AspNetCore.Http;

namespace OpenFinance.Shared.Infrastructure;

/// <summary>
/// Adds recommended security headers to all HTTP responses.
/// Protects against clickjacking, MIME sniffing, XSS, and other common web attacks.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;

            // Prevent clickjacking
            headers["X-Frame-Options"] = "DENY";

            // Prevent MIME type sniffing
            headers["X-Content-Type-Options"] = "nosniff";

            // Enable browser XSS filter
            headers["X-XSS-Protection"] = "1; mode=block";

            // Control referrer information
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Restrict browser features
            headers["Permissions-Policy"] = "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), usb=()";

            // Content Security Policy (API-only, restrictive)
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";

            // Enforce HTTPS (1 year, include subdomains)
            if (context.Request.IsHttps)
            {
                headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            }

            // Prevent caching of sensitive API responses
            headers["Cache-Control"] = "no-store";
            headers["Pragma"] = "no-cache";

            return Task.CompletedTask;
        });

        await _next(context);
    }
}
