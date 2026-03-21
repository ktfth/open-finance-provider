using Microsoft.EntityFrameworkCore;
using OpenFinance.InsuranceService.Application.UseCases;
using OpenFinance.InsuranceService.Domain.Repositories;
using OpenFinance.InsuranceService.Infrastructure.Persistence;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;
using DomainEntities = OpenFinance.InsuranceService.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Structured logging
builder.AddOpenFinanceSerilog("InsuranceService");

// Shared infrastructure
builder.Services.AddOpenFinanceInfrastructure(builder.Configuration);

// Authentication
builder.Services.AddOpenFinanceAuth(builder.Configuration);

// Distributed tracing
builder.Services.AddOpenFinanceTracing("InsuranceService", builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "OpenFinance Insurance Service",
        Version = "v1",
        Description = """
            Open Finance Brasil — Insurance API (Phase 4)

            Provides access to insurance policy data including:
            - Policy listings and full details (Life, Home, Auto, Health, Travel, and more)
            - Premium payment schedules with paid / outstanding aggregates
            - Claims history with approval status and amounts
            - Coverage lines (main and supplemental) with insured and deductible amounts

            ## Permissions Required
            | Endpoint | Permission |
            |----------|------------|
            | GET /insurances | INSURANCE_READ |
            | GET /insurances/{id} | INSURANCE_READ |
            | GET /insurances/{id}/premium | INSURANCE_PREMIUM_READ |
            | GET /insurances/{id}/claims | INSURANCE_CLAIMS_READ |
            | GET /insurances/{id}/coverages | INSURANCE_COVERAGES_READ |

            All requests must include a valid `x-consent-id` header referencing an active consent
            created via the Consent Service.
            """
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

builder.Services.AddDbContext<InsuranceDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("InsuranceDb"),
        sql => sql.EnableRetryOnFailure(3)));

builder.Services.AddDatabaseHealthCheck<InsuranceDbContext>();

builder.Services.AddConsentValidator(
    builder.Configuration["ConsentService:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration 'ConsentService:BaseUrl' is required."));

builder.Services.AddScoped<IInsuranceRepository, InsuranceRepository>();
builder.Services.AddScoped<GetInsurancesUseCase>();
builder.Services.AddScoped<GetInsuranceDetailsUseCase>();
builder.Services.AddScoped<GetInsurancePremiumUseCase>();
builder.Services.AddScoped<GetInsuranceClaimsUseCase>();
builder.Services.AddScoped<GetInsuranceCoveragesUseCase>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenFinance Insurance Service v1");
        c.RoutePrefix = "swagger";
    });
}

// Shared pipeline
app.UseOpenFinanceInfrastructure();

app.Run();

static void SeedDevelopmentData(InsuranceDbContext db, IWebHostEnvironment env)
{
    if (!env.IsDevelopment()) return;
    if (db.Insurances.Any()) return;

    // Sample life insurance policy for user-123
    var lifeInsurance = DomainEntities.Insurance.Create(
        userId: "user-123",
        type: InsuranceType.Life,
        productName: "Seguro de Vida Premium",
        insurerName: "Bradesco Seguros",
        insurerCnpj: "92.751.213/0001-73",
        policyNumber: "POL-VIDA-2024-00001",
        proposalDate: new DateOnly(2024, 1, 10),
        effectiveDate: new DateOnly(2024, 2, 1),
        expirationDate: new DateOnly(2025, 2, 1),
        insuredAmount: 500_000m,
        premiumAmount: 1_800m,
        currency: "BRL",
        gracePeriodDays: 30,
        insuredCpfCnpj: "123.456.789-00",
        insuredName: "João da Silva",
        beneficiaryName: "Maria da Silva");

    lifeInsurance.Activate();

    db.Insurances.Add(lifeInsurance);
    db.SaveChanges();

    // 2 coverages for the life insurance
    db.InsuranceCoverages.AddRange(
        DomainEntities.InsuranceCoverage.Create(
            insuranceId: lifeInsurance.Id,
            coverageName: "Morte por Qualquer Causa",
            type: CoverageType.Death,
            insuredAmount: 500_000m,
            deductibleAmount: 0m,
            currency: "BRL",
            isMainCoverage: true),
        DomainEntities.InsuranceCoverage.Create(
            insuranceId: lifeInsurance.Id,
            coverageName: "Invalidez Permanente Total por Acidente",
            type: CoverageType.Disability,
            insuredAmount: 500_000m,
            deductibleAmount: 0m,
            currency: "BRL",
            isMainCoverage: false)
    );
    db.SaveChanges();

    // 3 monthly premium payments: 1 paid, 1 overdue, 1 pending
    var payment1 = DomainEntities.InsurancePremiumPayment.Create(
        insuranceId: lifeInsurance.Id,
        dueDate: new DateOnly(2024, 2, 10),
        amount: 150m,
        currency: "BRL");
    payment1.MarkPaid();

    var payment2 = DomainEntities.InsurancePremiumPayment.Create(
        insuranceId: lifeInsurance.Id,
        dueDate: new DateOnly(2024, 3, 10),
        amount: 150m,
        currency: "BRL");
    payment2.MarkOverdue();

    var payment3 = DomainEntities.InsurancePremiumPayment.Create(
        insuranceId: lifeInsurance.Id,
        dueDate: new DateOnly(2024, 4, 10),
        amount: 150m,
        currency: "BRL");

    db.InsurancePremiumPayments.AddRange(payment1, payment2, payment3);
    db.SaveChanges();
}

public partial class Program { }
