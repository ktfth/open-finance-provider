using Microsoft.EntityFrameworkCore;
using OpenFinance.AccountService.Application.UseCases;
using OpenFinance.AccountService.Domain.Repositories;
using OpenFinance.AccountService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "OpenFinance Account Service", Version = "v1" });
});

builder.Services.AddDbContext<AccountDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("AccountDb"),
        sql => sql.EnableRetryOnFailure(3)));

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<GetAccountsUseCase>();
builder.Services.AddScoped<GetAccountDetailsUseCase>();
builder.Services.AddScoped<GetBalanceUseCase>();
builder.Services.AddScoped<GetTransactionsUseCase>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
    db.Database.EnsureCreated();
    SeedDevelopmentData(db, app.Environment);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

static void SeedDevelopmentData(AccountDbContext db, IWebHostEnvironment env)
{
    if (!env.IsDevelopment()) return;
    if (db.Accounts.Any()) return;

    var account1 = OpenFinance.AccountService.Domain.Entities.Account.Create(
        "user-123", "00012345-6", "0001",
        OpenFinance.Shared.Contracts.AccountType.Checking,
        "BRL", "João da Silva", "123.456.789-00");

    var account2 = OpenFinance.AccountService.Domain.Entities.Account.Create(
        "user-123", "00098765-4", "0001",
        OpenFinance.Shared.Contracts.AccountType.Savings,
        "BRL", "João da Silva", "123.456.789-00");

    db.Accounts.AddRange(account1, account2);
    db.SaveChanges();

    db.AccountBalances.AddRange(
        OpenFinance.AccountService.Domain.Entities.AccountBalance.Create(account1.Id, 5420.50m, 0m, "BRL"),
        OpenFinance.AccountService.Domain.Entities.AccountBalance.Create(account2.Id, 12800.00m, 500m, "BRL")
    );

    db.Transactions.AddRange(
        OpenFinance.AccountService.Domain.Entities.Transaction.Create(account1.Id, 3200m, "BRL", "CREDIT", "Salário", DateTime.UtcNow.AddDays(-5)),
        OpenFinance.AccountService.Domain.Entities.Transaction.Create(account1.Id, -150.30m, "BRL", "DEBIT", "Supermercado", DateTime.UtcNow.AddDays(-3)),
        OpenFinance.AccountService.Domain.Entities.Transaction.Create(account1.Id, -89.90m, "BRL", "DEBIT", "Farmácia", DateTime.UtcNow.AddDays(-1))
    );

    db.SaveChanges();
}

public partial class Program { }
