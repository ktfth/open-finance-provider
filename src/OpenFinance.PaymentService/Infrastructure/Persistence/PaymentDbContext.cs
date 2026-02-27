using Microsoft.EntityFrameworkCore;
using OpenFinance.PaymentService.Domain.Entities;

namespace OpenFinance.PaymentService.Infrastructure.Persistence;

public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(b =>
        {
            b.ToTable("Payments");
            b.HasKey(p => p.Id);
            b.Property(p => p.ConsentId).IsRequired();
            b.Property(p => p.DebtorAccountId).IsRequired().HasMaxLength(256);
            b.Property(p => p.CreditorAccountId).IsRequired().HasMaxLength(256);
            b.Property(p => p.CreditorName).IsRequired().HasMaxLength(256);
            b.Property(p => p.CreditorCpfCnpj).IsRequired().HasMaxLength(20);
            b.Property(p => p.Amount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(p => p.Currency).IsRequired().HasMaxLength(3);
            b.Property(p => p.Description).HasMaxLength(512);
            b.Property(p => p.Type).HasConversion<string>().HasMaxLength(20);
            b.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
            b.Property(p => p.CompletedAt);
            b.Property(p => p.FailureReason).HasMaxLength(512);
            b.HasIndex(p => p.ConsentId);
            b.HasIndex(p => p.Status);
        });
    }
}
