using System.IO.Compression;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace OpenFinance.Shared.Infrastructure;

/// <summary>
/// Shared service registration for all Open Finance microservices.
/// Provides consistent configuration for logging, health checks, rate limiting, CORS, and error handling.
/// Swagger must be configured per-service since Swashbuckle is a per-project NuGet dependency.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all shared Open Finance infrastructure services.
    /// Call this in each microservice's Program.cs BEFORE AddSwaggerGen.
    /// </summary>
    public static IServiceCollection AddOpenFinanceInfrastructure(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        // Controllers with standard JSON options and validation filter
        services.AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter>();
            })
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        // Suppress automatic 400 from [ApiController] — let ValidationFilter handle it
        services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        // ProblemDetails support
        services.AddProblemDetails();

        // Global exception handler
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // Health checks (liveness = self, readiness = overridden per-service with DB check)
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

        // Rate limiting (per IP, configurable or default 100 req/min)
        var permitLimit = configuration?.GetValue<int>("RateLimiting:PermitLimit") ?? 100;
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        // Response compression (Gzip for JSON payloads)
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<GzipCompressionProvider>();
        });
        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        // CORS — configurable origins, restrictive by default
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                var allowedOrigins = configuration?.GetSection("Cors:AllowedOrigins").Get<string[]>();
                if (allowedOrigins is { Length: > 0 })
                {
                    policy.WithOrigins(allowedOrigins);
                }
                else
                {
                    // Development fallback — restrict in production via config
                    policy.AllowAnyOrigin();
                }

                policy.AllowAnyMethod()
                      .WithHeaders("Content-Type", "Authorization", "x-consent-id", "x-idempotency-key", "x-correlation-id")
                      .WithExposedHeaders("x-correlation-id");
            });
        });

        return services;
    }

    /// <summary>
    /// Configures the shared Open Finance middleware pipeline.
    /// Call this in each microservice's Program.cs after Build().
    /// Swagger middleware must be added per-service before this call.
    /// </summary>
    public static WebApplication UseOpenFinanceInfrastructure(this WebApplication app)
    {
        // Response compression (must be before anything that writes response body)
        app.UseResponseCompression();

        // Exception handler (must be first after compression)
        app.UseExceptionHandler();

        // Security headers
        app.UseMiddleware<SecurityHeadersMiddleware>();

        // Request logging
        app.UseMiddleware<RequestLoggingMiddleware>();

        // Rate limiting
        app.UseRateLimiter();

        // CORS
        app.UseCors();

        app.UseHttpsRedirection();

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Health checks (anonymous — no auth required)
        app.MapHealthChecks("/health/live", new()
        {
            Predicate = check => check.Tags.Contains("live")
        }).AllowAnonymous();

        app.MapHealthChecks("/health/ready", new()
        {
            Predicate = check => check.Tags.Contains("ready")
        }).AllowAnonymous();

        app.MapControllers();

        return app;
    }
}
