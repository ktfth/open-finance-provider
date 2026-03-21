using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace OpenFinance.Shared.Consent;

/// <summary>
/// Extension methods to register consent validation in each microservice.
/// </summary>
public static class ConsentValidatorExtensions
{
    /// <summary>
    /// Registers IConsentValidator with an HttpClient pointing to the ConsentService.
    /// Includes Polly resilience: retry (3x exponential backoff), circuit breaker, and timeout.
    /// Usage: builder.Services.AddConsentValidator(builder.Configuration["ConsentService:BaseUrl"]!);
    /// </summary>
    public static IServiceCollection AddConsentValidator(
        this IServiceCollection services,
        string consentServiceBaseUrl)
    {
        services.AddHttpClient<IConsentValidator, HttpConsentValidator>(client =>
        {
            client.BaseAddress = new Uri(consentServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .AddStandardResilienceHandler(options =>
        {
            // Total request timeout (including retries)
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(8);

            // Retry: 3 attempts with exponential backoff
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.Delay = TimeSpan.FromMilliseconds(200);
            options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
            options.Retry.UseJitter = true;

            // Circuit breaker: open after 5 failures in 30s window
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.MinimumThroughput = 5;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);

            // Per-attempt timeout
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);
        });

        return services;
    }
}
