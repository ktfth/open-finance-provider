using Microsoft.EntityFrameworkCore;
using OpenFinance.PensionService.Application.UseCases;
using OpenFinance.PensionService.Domain.Entities;
using OpenFinance.PensionService.Domain.Repositories;
using OpenFinance.PensionService.Infrastructure.Persistence;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Structured logging
builder.AddOpenFinanceSerilog("PensionService");

// Shared infrastructure
builder.Services.AddOpenFinanceInfrastructure(builder.Configuration);

// Authentication
builder.Services.AddOpenFinanceAuth(builder.Configuration);

// Distributed tracing
builder.Services.AddOpenFinanceTracing("PensionService", builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "OpenFinance Pension Service",
        Version = "v1",
        Description = """
            Open Finance Brasil — Pension API (Phase 4)

            Provides access to supplementary pension plan data:

            ## Pension Types
            - **PGBL** — Plano Gerador de Benefício Livre (tax-deductible contributions)
            - **VGBL** — Vida Gerador de Benefício Livre (tax on yield only)
            - **PREVI** — Closed corporate pension funds
            - **Other** — Other supplementary pension products

            ## Tax Regimes
            - **Progressive** — Standard income tax table applied at withdrawal
            - **Regressive** — Declining IR rate (35% → 10%) based on accumulation time

            ## Available Resources
            | Resource | Description |
            |----------|-------------|
            | GET /pensions | List all pension plans for a user |
            | GET /pensions/{id} | Full plan details (fees, beneficiary, income type) |
            | GET /pensions/{id}/balance | Current financial position |
            | GET /pensions/{id}/contributions | All contribution records |
            | GET /pensions/{id}/withdrawals | All withdrawal and redemption events |

            ## Permissions Required
            All requests must include a valid `x-consent-id` header with PENSION_READ permission.
            """
    });
});

builder.Services.AddDbContext<PensionDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PensionDb"),
        sql => sql.EnableRetryOnFailure(3)));

builder.Services.AddDatabaseHealthCheck<PensionDbContext>();

builder.Services.AddConsentValidator(
    builder.Configuration["ConsentService:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration 'ConsentService:BaseUrl' is required."));

builder.Services.AddScoped<IPensionRepository, PensionRepository>();

builder.Services.AddScoped<GetPensionsUseCase>();
builder.Services.AddScoped<GetPensionDetailsUseCase>();
builder.Services.AddScoped<GetPensionBalanceUseCase>();
builder.Services.AddScoped<GetPensionContributionsUseCase>();
builder.Services.AddScoped<GetPensionWithdrawalsUseCase>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PensionDbContext>();
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenFinance Pension Service v1");
        c.RoutePrefix = "swagger";
    });
}

// Shared pipeline
app.UseOpenFinanceInfrastructure();

app.Run();

static void SeedDevelopmentData(PensionDbContext db, IWebHostEnvironment env)
{
    if (!env.IsDevelopment()) return;
    if (db.Pensions.Any()) return;

    // ── PGBL pension plan ────────────────────────────────────────────────────
    var pgbl = Pension.Create(
        "user-123",
        PensionType.PGBL,
        PensionModality.Contribution,
        "PGBL Bradesco Regressive 2045",
        "Bradesco Vida e Previdência S.A.",
        "51.990.695/0001-16",
        "CERT-PGBL-2021-00487",
        new DateOnly(2021, 3, 15),
        new DateOnly(2045, 3, 15),
        TaxRegimeType.Regressive,
        500.00m,
        PaymentFrequency.Monthly,
        "BRL",
        "Maria da Silva",
        1.50m,
        1.00m,
        IncomeType.LumpSum);

    db.Pensions.Add(pgbl);
    db.SaveChanges();

    // ── Balance for PGBL ─────────────────────────────────────────────────────
    var balance = PensionBalance.Create(
        pgbl.Id,
        DateOnly.FromDateTime(DateTime.UtcNow),
        42_800.00m,
        38_520.00m,
        36_000.00m,
        7_650.00m,
        642.00m,
        360.00m,
        "BRL");

    db.PensionBalances.Add(balance);
    db.SaveChanges();

    // ── Contributions ────────────────────────────────────────────────────────
    db.PensionContributions.AddRange(
        PensionContribution.Create(
            pgbl.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2)),
            500.00m,
            "BRL",
            ContributionType.Regular,
            ContributionStatus.Confirmed),
        PensionContribution.Create(
            pgbl.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            500.00m,
            "BRL",
            ContributionType.Regular,
            ContributionStatus.Confirmed),
        PensionContribution.Create(
            pgbl.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
            2_000.00m,
            "BRL",
            ContributionType.Extra,
            ContributionStatus.Confirmed));

    db.SaveChanges();
}

public partial class Program { }
