using Microsoft.EntityFrameworkCore;
using OpenFinance.ExchangeService.Domain.Entities;

namespace OpenFinance.ExchangeService.Infrastructure.Persistence;

public class ExchangeDbContext(DbContextOptions<ExchangeDbContext> options) : DbContext(options)
{
    public DbSet<ExchangeOperation> ExchangeOperations => Set<ExchangeOperation>();
    public DbSet<ExchangeEvent> ExchangeEvents => Set<ExchangeEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExchangeOperation>(b =>
        {
            b.ToTable("ExchangeOperations");
            b.HasKey(o => o.Id);
            b.Property(o => o.UserId).IsRequired().HasMaxLength(256);
            b.Property(o => o.OperationNumber).IsRequired().HasMaxLength(50);
            b.Property(o => o.OperationType).HasConversion<string>().HasMaxLength(30);
            b.Property(o => o.Category).HasConversion<string>().HasMaxLength(50);
            b.Property(o => o.Status).HasConversion<string>().HasMaxLength(30);
            b.Property(o => o.ForeignCurrency).IsRequired().HasMaxLength(3);
            b.Property(o => o.LocalCurrency).IsRequired().HasMaxLength(3);
            b.Property(o => o.ForeignCurrencyAmount).HasColumnType("decimal(18,6)");
            b.Property(o => o.LocalCurrencyAmount).HasColumnType("decimal(18,6)");
            b.Property(o => o.ExchangeRate).HasColumnType("decimal(18,6)");
            b.Property(o => o.VETAmount).HasColumnType("decimal(18,6)");
            b.Property(o => o.IOFAmount).HasColumnType("decimal(18,2)");
            b.Property(o => o.IRAmount).HasColumnType("decimal(18,2)");
            b.Property(o => o.CounterpartyName).IsRequired().HasMaxLength(256);
            b.Property(o => o.CounterpartyCountry).IsRequired().HasMaxLength(3);
            b.Property(o => o.DeliveryType).HasConversion<string>().HasMaxLength(30);
            b.HasIndex(o => o.UserId);
            b.HasIndex(o => o.OperationNumber).IsUnique();
            b.HasIndex(o => o.OperationDate);
        });

        modelBuilder.Entity<ExchangeEvent>(b =>
        {
            b.ToTable("ExchangeEvents");
            b.HasKey(e => e.Id);
            b.Property(e => e.OperationId).IsRequired();
            b.Property(e => e.EventType).HasConversion<string>().HasMaxLength(30);
            b.Property(e => e.ForeignCurrencyAmount).HasColumnType("decimal(18,6)");
            b.Property(e => e.LocalCurrencyAmount).HasColumnType("decimal(18,6)");
            b.Property(e => e.ForeignCurrency).IsRequired().HasMaxLength(3);
            b.Property(e => e.LocalCurrency).IsRequired().HasMaxLength(3);
            b.Property(e => e.ExchangeRate).HasColumnType("decimal(18,6)");
            b.HasIndex(e => e.OperationId);
            b.HasIndex(e => e.EventDate);
        });
    }
}
