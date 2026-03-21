using Microsoft.EntityFrameworkCore;
using OpenFinance.LoanService.Application.UseCases;
using OpenFinance.LoanService.Domain.Repositories;
using OpenFinance.LoanService.Infrastructure.Persistence;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Structured logging
builder.AddOpenFinanceSerilog("LoanService");

// Shared infrastructure (controllers, health checks, rate limiting, CORS, exception handler)
builder.Services.AddOpenFinanceInfrastructure(builder.Configuration);

// Authentication
builder.Services.AddOpenFinanceAuth(builder.Configuration);

// Distributed tracing
builder.Services.AddOpenFinanceTracing("LoanService", builder.Configuration);

// Swagger (per-service, Swashbuckle dependency)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "OpenFinance Loan Service", Version = "v1" });
});

// Database
builder.Services.AddDbContext<LoanDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("LoanDb"),
        sql => sql.EnableRetryOnFailure(3)));

builder.Services.AddDatabaseHealthCheck<LoanDbContext>();

// Consent validation
builder.Services.AddConsentValidator(
    builder.Configuration["ConsentService:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration 'ConsentService:BaseUrl' is required."));

// DI registrations
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<GetLoansUseCase>();
builder.Services.AddScoped<GetLoanDetailsUseCase>();
builder.Services.AddScoped<GetLoanPaymentsUseCase>();
builder.Services.AddScoped<GetLoanInstalmentsUseCase>();
builder.Services.AddScoped<GetLoanWarrantiesUseCase>();
builder.Services.AddScoped<GetFinancingsUseCase>();
builder.Services.AddScoped<GetFinancingDetailsUseCase>();
builder.Services.AddScoped<GetOverdraftsUseCase>();
builder.Services.AddScoped<GetOverdraftDetailsUseCase>();

var app = builder.Build();

// Database init
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
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

static void SeedDevelopmentData(LoanDbContext db, IWebHostEnvironment env)
{
    if (!env.IsDevelopment()) return;
    if (db.LoanContracts.Any()) return;

    var loan = OpenFinance.LoanService.Domain.Entities.LoanContract.Create(
        "user-123",
        "LN-2024-00001",
        OpenFinance.Shared.Contracts.LoanType.PersonalLoan,
        "Crédito Pessoal Padrão",
        "12.345.678/0001-99",
        25000.00m,
        18500.00m,
        0.0199m,
        OpenFinance.Shared.Contracts.InterestRateType.Compound,
        OpenFinance.Shared.Contracts.RateIndexer.CDI,
        "BRL",
        new DateOnly(2024, 1, 15),
        new DateOnly(2026, 1, 15),
        new DateOnly(2026, 1, 20),
        24,
        8,
        0.0245m,
        OpenFinance.Shared.Contracts.AmortizationType.Price);

    db.LoanContracts.Add(loan);
    db.SaveChanges();

    db.LoanInstalments.AddRange(
        OpenFinance.LoanService.Domain.Entities.LoanInstalment.Create(
            loan.Id, 1, new DateOnly(2024, 2, 15), 1250.00m, 1000.00m, 200.00m, 50.00m, "BRL",
            OpenFinance.Shared.Contracts.InstalmentStatus.Paid),
        OpenFinance.LoanService.Domain.Entities.LoanInstalment.Create(
            loan.Id, 2, new DateOnly(2024, 3, 15), 1250.00m, 1010.00m, 190.00m, 50.00m, "BRL",
            OpenFinance.Shared.Contracts.InstalmentStatus.Paid),
        OpenFinance.LoanService.Domain.Entities.LoanInstalment.Create(
            loan.Id, 9, new DateOnly(2024, 10, 15), 1250.00m, 1100.00m, 100.00m, 50.00m, "BRL",
            OpenFinance.Shared.Contracts.InstalmentStatus.Pending)
    );

    db.LoanWarranties.Add(
        OpenFinance.LoanService.Domain.Entities.LoanWarranty.Create(
            loan.Id, OpenFinance.Shared.Contracts.WarrantyType.Surety, "Aval pessoal", "BRL", 25000.00m)
    );

    db.LoanPayments.Add(
        OpenFinance.LoanService.Domain.Entities.LoanPayment.Create(
            loan.Id, new DateOnly(2024, 2, 15), 1250.00m, 1000.00m, 200.00m, 50.00m, 0.00m, "BRL", false)
    );

    var financing = OpenFinance.LoanService.Domain.Entities.FinancingContract.Create(
        "user-123",
        "FN-2024-00001",
        OpenFinance.Shared.Contracts.FinancingType.HomeFinancing,
        "Financiamento Habitacional",
        "12.345.678/0001-99",
        320000.00m,
        290000.00m,
        0.0089m,
        OpenFinance.Shared.Contracts.InterestRateType.Compound,
        OpenFinance.Shared.Contracts.RateIndexer.TR,
        "BRL",
        new DateOnly(2024, 3, 1),
        new DateOnly(2044, 3, 1),
        240,
        4,
        0.0112m,
        OpenFinance.Shared.Contracts.AmortizationType.SAC);

    db.FinancingContracts.Add(financing);
    db.SaveChanges();
}

public partial class Program { }
