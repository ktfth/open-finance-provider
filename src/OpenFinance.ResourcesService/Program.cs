using Microsoft.EntityFrameworkCore;
using OpenFinance.ResourcesService.Application.UseCases;
using OpenFinance.ResourcesService.Domain.Repositories;
using OpenFinance.ResourcesService.Infrastructure.Persistence;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Structured logging
builder.AddOpenFinanceSerilog("ResourcesService");

// Shared infrastructure (controllers, health checks, rate limiting, CORS, exception handler)
builder.Services.AddOpenFinanceInfrastructure(builder.Configuration);

// Authentication
builder.Services.AddOpenFinanceAuth(builder.Configuration);

// Distributed tracing
builder.Services.AddOpenFinanceTracing("ResourcesService", builder.Configuration);

// Swagger (per-service, Swashbuckle dependency)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "OpenFinance Resources Service", Version = "v1" });
});

// Database
builder.Services.AddDbContext<ResourceDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ResourcesDb"),
        sql => sql.EnableRetryOnFailure(3)));

builder.Services.AddDatabaseHealthCheck<ResourceDbContext>();

// Consent validation
builder.Services.AddConsentValidator(
    builder.Configuration["ConsentService:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration 'ConsentService:BaseUrl' is required."));

// DI registrations
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<GetResourcesUseCase>();

var app = builder.Build();

// Database init
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ResourceDbContext>();
    if (app.Environment.IsDevelopment())
        db.Database.EnsureCreated();
    else
        db.Database.Migrate();
    SeedDevelopmentData(db, app.Environment);
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

static void SeedDevelopmentData(ResourceDbContext db, IWebHostEnvironment env)
{
    if (!env.IsDevelopment()) return;
    if (db.Resources.Any()) return;

    var sampleConsentId = new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    var resources = new[]
    {
        OpenFinance.ResourcesService.Domain.Entities.Resource.Create(
            sampleConsentId, Guid.NewGuid(), ResourceType.Account),
        OpenFinance.ResourcesService.Domain.Entities.Resource.Create(
            sampleConsentId, Guid.NewGuid(), ResourceType.CreditCard),
        OpenFinance.ResourcesService.Domain.Entities.Resource.Create(
            sampleConsentId, Guid.NewGuid(), ResourceType.Loan),
        OpenFinance.ResourcesService.Domain.Entities.Resource.Create(
            sampleConsentId, Guid.NewGuid(), ResourceType.FixedIncome),
        OpenFinance.ResourcesService.Domain.Entities.Resource.Create(
            sampleConsentId, Guid.NewGuid(), ResourceType.VariableIncome),
        OpenFinance.ResourcesService.Domain.Entities.Resource.Create(
            sampleConsentId, Guid.NewGuid(), ResourceType.TreasuryBond),
    };

    db.Resources.AddRange(resources);
    db.SaveChanges();
}

public partial class Program { }
