using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace OpenFinance.Shared.Infrastructure;

/// <summary>
/// JWT Bearer authentication configuration for Open Finance services.
/// Requires configuration section "Authentication" with Issuer, Audience, and SigningKey.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Adds JWT Bearer authentication.
    /// Configuration keys: Authentication:Issuer, Authentication:Audience, Authentication:SigningKey.
    /// </summary>
    public static IServiceCollection AddOpenFinanceAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authSection = configuration.GetSection("Authentication");
        var signingKey = authSection["SigningKey"]
            ?? throw new InvalidOperationException("Authentication:SigningKey is required. Set via environment variable Authentication__SigningKey.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authSection["Issuer"] ?? "open-finance-provider",

                    ValidateAudience = true,
                    ValidAudience = authSection["Audience"] ?? "open-finance-api",

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(signingKey)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),

                    RequireExpirationTime = true,
                    RequireSignedTokens = true
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>()
                            .CreateLogger("OpenFinance.Auth");

                        logger.LogWarning(
                            "JWT authentication failed: {Error}",
                            context.Exception.Message);

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
