using Microsoft.EntityFrameworkCore;
using OpenFinance.InvestmentService.Application.UseCases;
using OpenFinance.InvestmentService.Domain.Entities;
using OpenFinance.InvestmentService.Domain.Repositories;
using OpenFinance.InvestmentService.Infrastructure.Persistence;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Structured logging
builder.AddOpenFinanceSerilog("InvestmentService");

// Shared infrastructure
builder.Services.AddOpenFinanceInfrastructure(builder.Configuration);

// Authentication
builder.Services.AddOpenFinanceAuth(builder.Configuration);

// Distributed tracing
builder.Services.AddOpenFinanceTracing("InvestmentService", builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "OpenFinance Investment Service",
        Version = "v1",
        Description = """
            Open Finance Brasil — Investments API

            Provides access to all investment categories:

            ## Fixed Income (Renda Fixa)
            CDB, LCI, LCA, CRI, CRA, Debêntures, Letra Financeira, Fixed Income Funds
            - Positions, balances (marked-to-market), and transaction history

            ## Variable Income (Renda Variável)
            Stocks (Ações), FIA, BDRs, ETFs, FIIs
            - Positions with average cost vs current price, tax calculation (IR 15%), and movements

            ## Treasury Bonds (Tesouro Direto)
            Tesouro Prefixado, Tesouro IPCA+, Tesouro Selic and their variants
            - Full position details with regressive income tax table (22.5% → 15%)

            ## Permissions Required
            | Resource | Permission |
            |----------|------------|
            | Fixed Income | INVESTMENTS_FIXED_INCOMES_READ |
            | Variable Income | INVESTMENTS_VARIABLE_INCOMES_READ |
            | Treasury Bonds | INVESTMENTS_TREASURE_TITLES_READ |

            All requests must include a valid `x-consent-id` header.
            """
    });
});

builder.Services.AddDbContext<InvestmentDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("InvestmentDb"),
        sql => sql.EnableRetryOnFailure(3)));

builder.Services.AddDatabaseHealthCheck<InvestmentDbContext>();

builder.Services.AddConsentValidator(
    builder.Configuration["ConsentService:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration 'ConsentService:BaseUrl' is required."));

builder.Services.AddScoped<IInvestmentRepository, InvestmentRepository>();

// Fixed Income use cases
builder.Services.AddScoped<GetFixedIncomeUseCase>();
builder.Services.AddScoped<GetFixedIncomeDetailsUseCase>();
builder.Services.AddScoped<GetFixedIncomeBalanceUseCase>();
builder.Services.AddScoped<GetFixedIncomeTransactionsUseCase>();

// Variable Income use cases
builder.Services.AddScoped<GetVariableIncomeUseCase>();
builder.Services.AddScoped<GetVariableIncomeDetailsUseCase>();
builder.Services.AddScoped<GetVariableIncomeBalanceUseCase>();
builder.Services.AddScoped<GetVariableIncomeTransactionsUseCase>();

// Treasury Bond use cases
builder.Services.AddScoped<GetTreasuryBondsUseCase>();
builder.Services.AddScoped<GetTreasuryBondDetailsUseCase>();
builder.Services.AddScoped<GetTreasuryBondBalanceUseCase>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InvestmentDbContext>();
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenFinance Investment Service v1");
        c.RoutePrefix = "swagger";
    });
}

// Shared pipeline
app.UseOpenFinanceInfrastructure();

app.Run();

static void SeedDevelopmentData(InvestmentDbContext db, IWebHostEnvironment env)
{
    if (!env.IsDevelopment()) return;
    if (db.FixedIncomeInvestments.Any()) return;

    // ── Fixed Income ──────────────────────────────────────────────────────────
    var cdb = FixedIncomeInvestment.Create(
        "user-123",
        FixedIncomeType.CDB,
        "CDB Banco Inter 120% CDI",
        "Banco Inter",
        "BRIN11CDB001",
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-180)),
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(180)),
        1000m, 1000m, 5m, "BRL",
        RateIndexer.CDI, 120m, 0m, 0m, 0m,
        RemunType.PostFixed);

    var lci = FixedIncomeInvestment.Create(
        "user-123",
        FixedIncomeType.LCI,
        "LCI Bradesco 95% CDI",
        "Banco Bradesco",
        "BBDC11LCI001",
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90)),
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(270)),
        1000m, 1000m, 3m, "BRL",
        RateIndexer.CDI, 95m, 0m, 0m, 100m,   // LCI is tax exempt
        RemunType.PostFixed);

    var cri = FixedIncomeInvestment.Create(
        "user-123",
        FixedIncomeType.CRI,
        "CRI Securitizadora IPCA + 7%",
        "Ápice Securitizadora",
        "APCE11CRI001",
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-365)),
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(730)),
        1000m, 1000m, 2m, "BRL",
        RateIndexer.IPCA, 100m, 7m, 0m, 100m,  // CRI is tax exempt
        RemunType.Hybrid);

    db.FixedIncomeInvestments.AddRange(cdb, lci, cri);
    db.SaveChanges();

    // Fixed income balances (marked-to-market)
    db.FixedIncomeBalances.AddRange(
        FixedIncomeBalance.Create(cdb.Id, DateOnly.FromDateTime(DateTime.UtcNow),
            5320m, 65.5m, 0m, 1000m, 1064m, 5m, "BRL"),
        FixedIncomeBalance.Create(lci.Id, DateOnly.FromDateTime(DateTime.UtcNow),
            3090m, 0m, 0m, 1000m, 1030m, 3m, "BRL"),
        FixedIncomeBalance.Create(cri.Id, DateOnly.FromDateTime(DateTime.UtcNow),
            2180m, 0m, 0m, 1000m, 1090m, 2m, "BRL")
    );
    db.SaveChanges();

    // Fixed income transactions
    db.FixedIncomeTransactions.AddRange(
        FixedIncomeTransaction.Create(cdb.Id, FixedIncomeTransactionType.Purchase,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-180)), 5m, 1000m, 5000m, 0m, "BRL"),
        FixedIncomeTransaction.Create(lci.Id, FixedIncomeTransactionType.Purchase,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90)), 3m, 1000m, 3000m, 0m, "BRL"),
        FixedIncomeTransaction.Create(cri.Id, FixedIncomeTransactionType.Purchase,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-365)), 2m, 1000m, 2000m, 0m, "BRL"),
        FixedIncomeTransaction.Create(cri.Id, FixedIncomeTransactionType.InterestPayment,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-180)), 0m, 0m, 90m, 0m, "BRL")
    );
    db.SaveChanges();

    // ── Variable Income ───────────────────────────────────────────────────────
    var petr4 = VariableIncomeInvestment.Create(
        "user-123",
        VariableIncomeType.Stock,
        "PETR4",
        "Petróleo Brasileiro S.A. - Petrobras",
        "BRPETRACNPR6",
        100m, 32.50m, 38.20m, "BRL",
        DateOnly.FromDateTime(DateTime.UtcNow));

    var vale3 = VariableIncomeInvestment.Create(
        "user-123",
        VariableIncomeType.Stock,
        "VALE3",
        "Vale S.A.",
        "BRVALEACNOR0",
        50m, 68.00m, 72.50m, "BRL",
        DateOnly.FromDateTime(DateTime.UtcNow));

    var bova11 = VariableIncomeInvestment.Create(
        "user-123",
        VariableIncomeType.ETF,
        "BOVA11",
        "iShares Ibovespa Fundo de Índice",
        "BRBOVA11CTF0",
        30m, 115.00m, 118.75m, "BRL",
        DateOnly.FromDateTime(DateTime.UtcNow));

    var mxrf11 = VariableIncomeInvestment.Create(
        "user-123",
        VariableIncomeType.FII,
        "MXRF11",
        "Maxi Renda FII",
        "BRMXRFCTF001",
        200m, 9.80m, 10.25m, "BRL",
        DateOnly.FromDateTime(DateTime.UtcNow));

    db.VariableIncomeInvestments.AddRange(petr4, vale3, bova11, mxrf11);
    db.SaveChanges();

    // Variable income transactions
    db.VariableIncomeTransactions.AddRange(
        VariableIncomeTransaction.Create(petr4.Id, VariableIncomeTransactionType.Buy,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-120)), 100m, 32.50m, 3250m, 5.90m, 0m, "BRL"),
        VariableIncomeTransaction.Create(petr4.Id, VariableIncomeTransactionType.DividendPayment,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)), 0m, 0m, 180m, 0m, 0m, "BRL"),
        VariableIncomeTransaction.Create(vale3.Id, VariableIncomeTransactionType.Buy,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90)), 50m, 68.00m, 3400m, 5.90m, 0m, "BRL"),
        VariableIncomeTransaction.Create(bova11.Id, VariableIncomeTransactionType.Buy,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-60)), 30m, 115.00m, 3450m, 5.90m, 0m, "BRL"),
        VariableIncomeTransaction.Create(mxrf11.Id, VariableIncomeTransactionType.Buy,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-200)), 200m, 9.80m, 1960m, 2.90m, 0m, "BRL"),
        VariableIncomeTransaction.Create(mxrf11.Id, VariableIncomeTransactionType.InterestOnCapital,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)), 0m, 0m, 200m, 0m, 0m, "BRL")
    );
    db.SaveChanges();

    // ── Treasury Bonds (Tesouro Direto) ───────────────────────────────────────
    var tesouroSelic = TreasuryBondInvestment.Create(
        "user-123",
        "Tesouro Selic 2027",
        TreasuryBondType.Selic,
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-400)),
        new DateOnly(2027, 3, 1),
        2m, 13456.78m, 14102.33m, "BRL",
        13.75m, 11.75m);

    var tesouroIPCA = TreasuryBondInvestment.Create(
        "user-123",
        "Tesouro IPCA+ 2029",
        TreasuryBondType.IPCAMais,
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-200)),
        new DateOnly(2029, 5, 15),
        1m, 3980.25m, 4215.60m, "BRL",
        6.30m, 4.62m);

    var tesouroPre = TreasuryBondInvestment.Create(
        "user-123",
        "Tesouro Prefixado 2026",
        TreasuryBondType.Prefixado,
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-100)),
        new DateOnly(2026, 1, 1),
        3m, 1000m, 1048.50m, "BRL",
        13.50m, 0m);

    db.TreasuryBondInvestments.AddRange(tesouroSelic, tesouroIPCA, tesouroPre);
    db.SaveChanges();
}

public partial class Program { }
