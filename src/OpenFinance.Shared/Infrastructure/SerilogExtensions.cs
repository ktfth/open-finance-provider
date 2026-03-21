using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace OpenFinance.Shared.Infrastructure;

/// <summary>
/// Serilog configuration for structured JSON logging across all services.
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Configures Serilog as the logging provider with structured JSON output.
    /// Call on WebApplicationBuilder before Build().
    /// </summary>
    public static WebApplicationBuilder AddOpenFinanceSerilog(
        this WebApplicationBuilder builder,
        string serviceName)
    {
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", serviceName)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

            if (context.HostingEnvironment.IsDevelopment())
            {
                configuration.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Service} | {Message:lj}{NewLine}{Exception}");
            }
            else
            {
                configuration.WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter());
            }
        });

        return builder;
    }
}
