using Microsoft.EntityFrameworkCore;
using OpenFinance.CustomerService.Application.UseCases;
using OpenFinance.CustomerService.Domain.Repositories;
using OpenFinance.CustomerService.Infrastructure.Persistence;
using OpenFinance.Shared.Consent;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;
using DomainEntities = OpenFinance.CustomerService.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Structured logging
builder.AddOpenFinanceSerilog("CustomerService");

// Shared infrastructure (controllers, health checks, rate limiting, CORS, exception handler)
builder.Services.AddOpenFinanceInfrastructure(builder.Configuration);

// Authentication
builder.Services.AddOpenFinanceAuth(builder.Configuration);

// Distributed tracing
builder.Services.AddOpenFinanceTracing("CustomerService", builder.Configuration);

// Swagger (per-service, Swashbuckle dependency)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "OpenFinance Customer Service", Version = "v1" });
});

// Database
builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CustomerDb"),
        sql => sql.EnableRetryOnFailure(3)));

builder.Services.AddDatabaseHealthCheck<CustomerDbContext>();

// Consent validation
builder.Services.AddConsentValidator(
    builder.Configuration["ConsentService:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration 'ConsentService:BaseUrl' is required."));

// DI registrations
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<GetPersonalIdentificationUseCase>();
builder.Services.AddScoped<GetPersonalQualificationUseCase>();
builder.Services.AddScoped<GetBusinessIdentificationUseCase>();
builder.Services.AddScoped<GetBusinessQualificationUseCase>();

var app = builder.Build();

// Database init
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
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

static void SeedDevelopmentData(CustomerDbContext db, IWebHostEnvironment env)
{
    if (!env.IsDevelopment()) return;
    if (db.PersonalCustomers.Any()) return;

    var customer = DomainEntities.PersonalCustomer.Create(
        "user-123",
        "123.456.789-00",
        "João da Silva",
        "1985-06-15",
        MaritalStatusType.Married,
        SexType.Male,
        "Brazilian",
        "Brazil");

    db.PersonalCustomers.Add(customer);

    var qualification = DomainEntities.PersonalQualification.Create(
        "user-123",
        OccupationType.Employee,
        "Software Engineer",
        InformedIncomeFrequency.Monthly,
        15000.00m,
        "BRL",
        DateTime.UtcNow.AddMonths(-1));

    db.PersonalQualifications.Add(qualification);

    var address = DomainEntities.CustomerAddress.Create(
        "user-123",
        "Avenida Paulista",
        "1000",
        "Apto 42",
        "Bela Vista",
        "São Paulo",
        "SP",
        "01310-100",
        "Brasil",
        AddressType.Residential);

    db.CustomerAddresses.Add(address);

    var contact = DomainEntities.CustomerContact.Create(
        "user-123",
        PhoneType.Mobile,
        "55",
        "11",
        "999887766",
        "joao.silva@example.com",
        true);

    db.CustomerContacts.Add(contact);

    db.SaveChanges();
}

public partial class Program { }
