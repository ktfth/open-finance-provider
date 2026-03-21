using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OpenFinance.Shared.Infrastructure;

/// <summary>
/// Database health check extensions for the readiness probe.
/// Each service should call AddDatabaseHealthCheck&lt;TDbContext&gt; to verify DB connectivity.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds a database connectivity health check to the "ready" tag.
    /// The readiness probe will fail if the database is unreachable.
    /// </summary>
    public static IServiceCollection AddDatabaseHealthCheck<TDbContext>(
        this IServiceCollection services,
        string name = "database")
        where TDbContext : DbContext
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck<TDbContext>>(name, tags: ["ready"]);

        return services;
    }
}

/// <summary>
/// Health check that verifies database connectivity by executing a simple query.
/// </summary>
internal sealed class DatabaseHealthCheck<TDbContext>(TDbContext dbContext)
    : IHealthCheck where TDbContext : DbContext
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Database.CanConnectAsync(cancellationToken);
            return HealthCheckResult.Healthy("Database connection is healthy.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database connection failed.",
                exception: ex);
        }
    }
}
