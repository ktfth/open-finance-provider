using Microsoft.EntityFrameworkCore;
using OpenFinance.CreditCardService.Application.UseCases;
using OpenFinance.CreditCardService.Domain.Entities;
using OpenFinance.CreditCardService.Domain.Repositories;
using OpenFinance.CreditCardService.Infrastructure.Persistence;
using OpenFinance.Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "OpenFinance Credit Card Service",
        Version = "v1",
        Description = """
            Open Finance Brasil — Credit Cards API

            Provides access to credit card account data including:
            - Card account listings and details
            - Credit limit lines (total, national, international)
            - Monthly bills (faturas) and their status
            - Individual bill transactions

            ## Permissions Required
            | Endpoint | Permission |
            |----------|------------|
            | GET /credit-cards-accounts | CREDIT_CARDS_ACCOUNTS_READ |
            | GET /credit-cards-accounts/{id} | CREDIT_CARDS_ACCOUNTS_READ |
            | GET /credit-cards-accounts/{id}/limits | CREDIT_CARDS_ACCOUNTS_LIMITS_READ |
            | GET /credit-cards-accounts/{id}/bills | CREDIT_CARDS_ACCOUNTS_BILLS_READ |
            | GET /credit-cards-accounts/{id}/bills/{billId}/transactions | CREDIT_CARDS_ACCOUNTS_BILLS_TRANSACTIONS_READ |

            All requests must include a valid `x-consent-id` header referencing an active consent
            created via the Consent Service.
            """
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

builder.Services.AddDbContext<CardDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CardDb"),
        sql => sql.EnableRetryOnFailure(3)));

builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<GetCardAccountsUseCase>();
builder.Services.AddScoped<GetCardAccountDetailsUseCase>();
builder.Services.AddScoped<GetCardLimitsUseCase>();
builder.Services.AddScoped<GetCardBillsUseCase>();
builder.Services.AddScoped<GetCardBillTransactionsUseCase>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CardDbContext>();
    db.Database.EnsureCreated();
    SeedDevelopmentData(db, app.Environment);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenFinance Credit Card Service v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

static void SeedDevelopmentData(CardDbContext db, IWebHostEnvironment env)
{
    if (!env.IsDevelopment()) return;
    if (db.CardAccounts.Any()) return;

    // Seed card accounts
    var card1 = CardAccount.Create(
        "user-123", "4321", CardBrand.Visa, CardType.Credit, CardNetworkType.Visa,
        "João da Silva", "123.456.789-00", paymentDay: 10);

    var card2 = CardAccount.Create(
        "user-123", "8765", CardBrand.Mastercard, CardType.Multiple, CardNetworkType.Mastercard,
        "João da Silva", "123.456.789-00", paymentDay: 20);

    db.CardAccounts.AddRange(card1, card2);
    db.SaveChanges();

    // Seed credit limits for card1
    var limit1Total = CardLimit.Create(
        card1.Id, CreditLimitType.Total, "LIMITE_CREDITO_TOTAL", "CONSOLIDADO",
        "LMT-001", "Limite Total", isLimitFlexible: false, limitAmountTotal: 15000m, "BRL");
    limit1Total.UpdateUsage(3240.50m);

    var limit1National = CardLimit.Create(
        card1.Id, CreditLimitType.Individual, "LIMITE_CREDITO_NACIONAL", "INDIVIDUAL",
        "LMT-002", "Limite Nacional", isLimitFlexible: false, limitAmountTotal: 12000m, "BRL");
    limit1National.UpdateUsage(2240.50m);

    var limit1International = CardLimit.Create(
        card1.Id, CreditLimitType.Individual, "LIMITE_CREDITO_INTERNACIONAL", "INDIVIDUAL",
        "LMT-003", "Limite Internacional", isLimitFlexible: true, limitAmountTotal: 3000m, "BRL");
    limit1International.UpdateUsage(1000m);

    // Seed limits for card2
    var limit2Total = CardLimit.Create(
        card2.Id, CreditLimitType.Total, "LIMITE_CREDITO_TOTAL", "CONSOLIDADO",
        "LMT-004", "Limite Total", isLimitFlexible: false, limitAmountTotal: 25000m, "BRL");
    limit2Total.UpdateUsage(8500m);

    db.CardLimits.AddRange(limit1Total, limit1National, limit1International, limit2Total);
    db.SaveChanges();

    // Seed bills for card1
    var bill1 = CardBill.Create(
        card1.Id,
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)),
        3240.50m, 324.05m, "BRL");
    bill1.Close();
    bill1.MarkPaid();

    var bill2 = CardBill.Create(
        card1.Id,
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
        1820.30m, 182.03m, "BRL");

    // Seed bills for card2
    var bill3 = CardBill.Create(
        card2.Id,
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
        8500m, 850m, "BRL");
    bill3.Close();

    db.CardBills.AddRange(bill1, bill2, bill3);
    db.SaveChanges();

    // Seed transactions for bill1 (card1, paid)
    db.CardTransactions.AddRange(
        CardTransaction.Create(card1.Id, bill1.Id, "TXN-001", "CREDITO_A_VISTA", "Supermercado Pão de Açúcar",
            bill1.Id.ToString(), CardTransactionType.Purchase, 256.80m, "BRL",
            DateTime.UtcNow.AddDays(-50), 256.80m, 5411),
        CardTransaction.Create(card1.Id, bill1.Id, "TXN-002", "CREDITO_A_VISTA", "Posto de Combustível Shell",
            bill1.Id.ToString(), CardTransactionType.Purchase, 180.00m, "BRL",
            DateTime.UtcNow.AddDays(-45), 180.00m, 5541),
        CardTransaction.Create(card1.Id, bill1.Id, "TXN-003", "CREDITO_PARCELADO_LOJA", "Amazon.com.br - 3x",
            bill1.Id.ToString(), CardTransactionType.Instalment, 399.90m, "BRL",
            DateTime.UtcNow.AddDays(-42), 133.30m, 5999),
        CardTransaction.Create(card1.Id, bill1.Id, "TXN-004", "CREDITO_A_VISTA", "iFood",
            bill1.Id.ToString(), CardTransactionType.Purchase, 89.50m, "BRL",
            DateTime.UtcNow.AddDays(-38), 89.50m, 5812),
        CardTransaction.Create(card1.Id, bill1.Id, "TXN-005", "ANUIDADE", "Anuidade Cartão Platinum",
            bill1.Id.ToString(), CardTransactionType.Fee, 35.83m, "BRL",
            DateTime.UtcNow.AddDays(-35), 35.83m, 6012)
    );

    // Seed transactions for bill2 (card1, current open bill)
    db.CardTransactions.AddRange(
        CardTransaction.Create(card1.Id, bill2.Id, "TXN-006", "CREDITO_A_VISTA", "Farmácia Droga Raia",
            bill2.Id.ToString(), CardTransactionType.Purchase, 142.60m, "BRL",
            DateTime.UtcNow.AddDays(-15), 142.60m, 5912),
        CardTransaction.Create(card1.Id, bill2.Id, "TXN-007", "CREDITO_A_VISTA", "Uber",
            bill2.Id.ToString(), CardTransactionType.Purchase, 35.90m, "BRL",
            DateTime.UtcNow.AddDays(-10), 35.90m, 4121),
        CardTransaction.Create(card1.Id, bill2.Id, "TXN-008", "CREDITO_A_VISTA", "Netflix",
            bill2.Id.ToString(), CardTransactionType.Purchase, 55.90m, "BRL",
            DateTime.UtcNow.AddDays(-5), 55.90m, 7841),
        CardTransaction.Create(card1.Id, bill2.Id, "TXN-009", "CREDITO_PARCELADO_LOJA", "Magazine Luiza - 6x",
            bill2.Id.ToString(), CardTransactionType.Instalment, 299.99m, "BRL",
            DateTime.UtcNow.AddDays(-3), 49.99m, 5722)
    );

    db.SaveChanges();
}

public partial class Program { }
