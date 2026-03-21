using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OpenFinance.Shared.Infrastructure;

/// <summary>
/// OpenTelemetry distributed tracing configuration for Open Finance services.
/// Traces ASP.NET Core requests, HTTP client calls, and EF Core queries.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Adds OpenTelemetry tracing with ASP.NET Core, HttpClient, and optional OTLP export.
    /// Set OpenTelemetry:Endpoint in configuration to enable OTLP exporter.
    /// </summary>
    public static IServiceCollection AddOpenFinanceTracing(
        this IServiceCollection services,
        string serviceName,
        IConfiguration configuration)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production"
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Don't trace health check endpoints
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation();

                // OTLP exporter (Jaeger, Zipkin, Azure Monitor, etc.)
                var otlpEndpoint = configuration["OpenTelemetry:Endpoint"];
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            });

        return services;
    }
}
