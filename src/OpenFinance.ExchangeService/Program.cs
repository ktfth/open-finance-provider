using Microsoft.EntityFrameworkCore;
using OpenFinance.ExchangeService.Application.UseCases;
using OpenFinance.ExchangeService.Domain.Entities;
using OpenFinance.ExchangeService.Domain.Repositories;
using OpenFinance.ExchangeService.Infrastructure.Persistence;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Structured logging
builder.AddOpenFinanceSerilog("ExchangeService");

// Shared infrastructure
builder.Services.AddOpenFinanceInfrastructure(builder.Configuration);

// Authentication
builder.Services.AddOpenFinanceAuth(builder.Configuration);

// Distributed tracing
builder.Services.AddOpenFinanceTracing("ExchangeService", builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "OpenFinance Exchange Service",
        Version = "v1",
        Description = """
            Open Finance Brasil — Foreign Exchange (Câmbio) API (Phase 4)

            Provides access to foreign exchange operations contracted by bank clients:

            ## Exchange Operations
            Purchase and sale of foreign currency, international transfers,
            import/export settlements and travel money.
            - Operation summaries with status, currencies and amounts
            - Full details including exchange rate, VET, IOF, IR and counterparty

            ## Exchange Events
            Lifecycle events for each operation:
            - Closing, Settlement, Partial Settlement, Cancellation, Amendment

            ## Permissions Required
            | Resource | Permission |
            |----------|------------|
            | Exchange Operations | EXCHANGES_READ |

            All requests must include a valid `x-consent-id` header.
            """
    });
});

builder.Services.AddDbContext<ExchangeDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ExchangeDb"),
        sql => sql.EnableRetryOnFailure(3)));

builder.Services.AddDatabaseHealthCheck<ExchangeDbContext>();

builder.Services.AddConsentValidator(
    builder.Configuration["ConsentService:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration 'ConsentService:BaseUrl' is required."));

builder.Services.AddScoped<IExchangeRepository, ExchangeRepository>();

builder.Services.AddScoped<GetExchangeOperationsUseCase>();
builder.Services.AddScoped<GetExchangeOperationDetailsUseCase>();
builder.Services.AddScoped<GetExchangeEventsUseCase>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ExchangeDbContext>();
    if (app.Environment.IsDevelopment())
        db.Database.EnsureCreated();
    else
        db.Database.Migrate();
    SeedDevelopmentData(db, app.Environment);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenFinance Exchange Service v1");
        c.RoutePrefix = "swagger";
    });
}

// Shared pipeline
app.UseOpenFinanceInfrastructure();

app.Run();

static void SeedDevelopmentData(ExchangeDbContext db, IWebHostEnvironment env)
{
    if (!env.IsDevelopment()) return;
    if (db.ExchangeOperations.Any()) return;

    // ── USD Purchase operation ─────────────────────────────────────────────────
    var usdPurchase = ExchangeOperation.Create(
        userId: "user-123",
        operationNumber: "CAM-2026-000001",
        operationType: ExchangeOperationType.Purchase,
        category: ExchangeCategory.Travel,
        foreignCurrency: "USD",
        localCurrency: "BRL",
        operationDate: new DateOnly(2026, 3, 1),
        deliveryDate: new DateOnly(2026, 3, 3),
        foreignCurrencyAmount: 5000m,
        localCurrencyAmount: 25750m,
        exchangeRate: 5.15m,
        vetAmount: 5.18m,
        iofAmount: 154.50m,
        irAmount: 0m,
        counterpartyName: "Banco do Brasil S.A.",
        counterpartyCountry: "BRA",
        deliveryType: ExchangeDeliveryType.Cash);

    db.ExchangeOperations.Add(usdPurchase);
    db.SaveChanges();

    // ── Events for the USD purchase operation ─────────────────────────────────
    db.ExchangeEvents.AddRange(
        ExchangeEvent.Create(
            operationId: usdPurchase.Id,
            eventType: ExchangeEventType.Closing,
            eventDate: new DateOnly(2026, 3, 1),
            foreignCurrencyAmount: 5000m,
            localCurrencyAmount: 25750m,
            foreignCurrency: "USD",
            localCurrency: "BRL",
            exchangeRate: 5.15m),
        ExchangeEvent.Create(
            operationId: usdPurchase.Id,
            eventType: ExchangeEventType.Settlement,
            eventDate: new DateOnly(2026, 3, 3),
            foreignCurrencyAmount: 5000m,
            localCurrencyAmount: 25750m,
            foreignCurrency: "USD",
            localCurrency: "BRL",
            exchangeRate: 5.15m)
    );
    db.SaveChanges();
}

public partial class Program { }
