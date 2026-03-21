using Microsoft.EntityFrameworkCore;
using OpenFinance.CapitalizationService.Application.UseCases;
using OpenFinance.CapitalizationService.Domain.Entities;
using OpenFinance.CapitalizationService.Domain.Repositories;
using OpenFinance.CapitalizationService.Infrastructure.Persistence;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Structured logging
builder.AddOpenFinanceSerilog("CapitalizationService");

// Shared infrastructure
builder.Services.AddOpenFinanceInfrastructure(builder.Configuration);

// Authentication
builder.Services.AddOpenFinanceAuth(builder.Configuration);

// Distributed tracing
builder.Services.AddOpenFinanceTracing("CapitalizationService", builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "OpenFinance Capitalization Service",
        Version = "v1",
        Description = """
            Open Finance Brasil — Capitalization API (Phase 4)

            Provides access to capitalization bond (título de capitalização) data for bank participants.

            ## Bond Modalities
            - **Traditional** — Standard capitalization with periodic prize draws
            - **Incentive** — Sold as incentive for purchasing goods or services
            - **Popular** — Low-cost bonds for financial inclusion
            - **CompulsoryPurchase** — Required as part of a financing or service agreement

            ## Key Features
            - Payment instalment schedules with due dates and amounts
            - Mathematical reserve and surrender quota calculations
            - Late payment fine and interest tracking
            - Redemption percentage and current redemption value
            - Prize draw amounts aligned with SUSEP regulations

            ## Permissions Required
            | Resource | Permission |
            |----------|------------|
            | Capitalization Bonds | CAPITALIZATION_TITLES_READ |

            All requests must include a valid `x-consent-id` header.
            """
    });
});

builder.Services.AddDbContext<CapitalizationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CapitalizationDb"),
        sql => sql.EnableRetryOnFailure(3)));

builder.Services.AddDatabaseHealthCheck<CapitalizationDbContext>();

builder.Services.AddConsentValidator(
    builder.Configuration["ConsentService:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration 'ConsentService:BaseUrl' is required."));

builder.Services.AddScoped<ICapitalizationRepository, CapitalizationRepository>();

builder.Services.AddScoped<GetCapitalizationBondsUseCase>();
builder.Services.AddScoped<GetCapitalizationBondDetailsUseCase>();
builder.Services.AddScoped<GetCapitalizationBondPaymentsUseCase>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CapitalizationDbContext>();
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenFinance Capitalization Service v1");
        c.RoutePrefix = "swagger";
    });
}

// Shared pipeline
app.UseOpenFinanceInfrastructure();

app.Run();

static void SeedDevelopmentData(CapitalizationDbContext db, IWebHostEnvironment env)
{
    if (!env.IsDevelopment()) return;
    if (db.CapitalizationBonds.Any()) return;

    // ── Traditional Capitalization Bond ───────────────────────────────────────
    var traditionalBond = CapitalizationBond.Create(
        userId: "user-123",
        bondNumber: "CAP-2024-000001",
        modality: CapitalizationModality.Traditional,
        productName: "Título Capitalização Tradicional Plus",
        companyName: "Brasilcap Capitalização S.A.",
        companyCnpj: "15138322000180",
        contractDate: new DateOnly(2024, 1, 15),
        maturityDate: new DateOnly(2026, 1, 15),
        paymentCount: 24,
        paymentAmount: 150.00m,
        paymentFrequency: PaymentFrequency.Monthly,
        latePaymentFine: 2.00m,
        latePaymentInterest: 1.00m,
        redemptionPercentage: 70.00m,
        currentRedemptionValue: 2520.00m,
        prizeDrawAmount: 50000.00m,
        currency: "BRL",
        totalPaidAmount: 1800.00m,
        mathematicalReserve: 1260.00m,
        surrenderQuota: 0.7000m);

    db.CapitalizationBonds.Add(traditionalBond);
    db.SaveChanges();

    // ── Payments for the Traditional Bond ─────────────────────────────────────
    var payments = new List<CapitalizationPayment>
    {
        CapitalizationPayment.Create(
            bondId: traditionalBond.Id,
            dueDate: new DateOnly(2024, 1, 15),
            amount: 150.00m,
            currency: "BRL",
            status: CapitalizationPaymentStatus.Paid),

        CapitalizationPayment.Create(
            bondId: traditionalBond.Id,
            dueDate: new DateOnly(2024, 2, 15),
            amount: 150.00m,
            currency: "BRL",
            status: CapitalizationPaymentStatus.Paid),

        CapitalizationPayment.Create(
            bondId: traditionalBond.Id,
            dueDate: new DateOnly(2024, 3, 15),
            amount: 150.00m,
            currency: "BRL",
            status: CapitalizationPaymentStatus.Paid)
    };

    db.CapitalizationPayments.AddRange(payments);
    db.SaveChanges();
}

public partial class Program { }
