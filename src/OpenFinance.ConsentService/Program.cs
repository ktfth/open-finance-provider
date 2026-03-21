using Microsoft.EntityFrameworkCore;
using OpenFinance.ConsentService.Application.UseCases;
using OpenFinance.ConsentService.Domain.Repositories;
using OpenFinance.ConsentService.Infrastructure.Persistence;
using OpenFinance.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Structured logging
builder.AddOpenFinanceSerilog("ConsentService");

// Shared infrastructure (controllers, health checks, rate limiting, CORS, exception handler)
builder.Services.AddOpenFinanceInfrastructure(builder.Configuration);

// Authentication
builder.Services.AddOpenFinanceAuth(builder.Configuration);

// Distributed tracing
builder.Services.AddOpenFinanceTracing("ConsentService", builder.Configuration);

// Swagger (per-service, Swashbuckle dependency)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "OpenFinance Consent Service", Version = "v1" });
});

// Database
builder.Services.AddDbContext<ConsentDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ConsentDb"),
        sql => sql.EnableRetryOnFailure(3)));

builder.Services.AddDatabaseHealthCheck<ConsentDbContext>();

// DI registrations
builder.Services.AddScoped<IConsentRepository, ConsentRepository>();
builder.Services.AddScoped<CreateConsentUseCase>();
builder.Services.AddScoped<GetConsentUseCase>();
builder.Services.AddScoped<RevokeConsentUseCase>();
builder.Services.AddScoped<ValidateConsentUseCase>();

var app = builder.Build();

// Database init
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ConsentDbContext>();
    if (app.Environment.IsDevelopment())
        db.Database.EnsureCreated();
    else
        db.Database.Migrate();
}

// Swagger (dev only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Shared middleware pipeline (exception handler, logging, rate limiting, CORS, HTTPS, health checks, controllers)
app.UseOpenFinanceInfrastructure();

app.Run();

public partial class Program { }
