using Microsoft.EntityFrameworkCore;
using OpenFinance.ConsentService.Application.UseCases;
using OpenFinance.ConsentService.Domain.Repositories;
using OpenFinance.ConsentService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "OpenFinance Consent Service", Version = "v1" });
});

builder.Services.AddDbContext<ConsentDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ConsentDb"),
        sql => sql.EnableRetryOnFailure(3)));

builder.Services.AddScoped<IConsentRepository, ConsentRepository>();
builder.Services.AddScoped<CreateConsentUseCase>();
builder.Services.AddScoped<GetConsentUseCase>();
builder.Services.AddScoped<RevokeConsentUseCase>();
builder.Services.AddScoped<ValidateConsentUseCase>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ConsentDbContext>().Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program { }
