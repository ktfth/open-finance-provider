using Microsoft.EntityFrameworkCore;
using OpenFinance.CapitalizationService.Domain.Entities;

namespace OpenFinance.CapitalizationService.Infrastructure.Persistence;

public class CapitalizationDbContext(DbContextOptions<CapitalizationDbContext> options) : DbContext(options)
{
    public DbSet<CapitalizationBond> CapitalizationBonds => Set<CapitalizationBond>();
    public DbSet<CapitalizationPayment> CapitalizationPayments => Set<CapitalizationPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CapitalizationBond>(b =>
        {
            b.ToTable("CapitalizationBonds");
            b.HasKey(x => x.Id);
            b.Property(x => x.UserId).IsRequired().HasMaxLength(256);
            b.Property(x => x.BondNumber).IsRequired().HasMaxLength(50);
            b.Property(x => x.Modality).HasConversion<string>().HasMaxLength(30);
            b.Property(x => x.ProductName).IsRequired().HasMaxLength(256);
            b.Property(x => x.CompanyName).IsRequired().HasMaxLength(256);
            b.Property(x => x.CompanyCnpj).IsRequired().HasMaxLength(14);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            b.Property(x => x.PaymentAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.PaymentFrequency).HasConversion<string>().HasMaxLength(20);
            b.Property(x => x.LatePaymentFine).HasColumnType("decimal(10,4)");
            b.Property(x => x.LatePaymentInterest).HasColumnType("decimal(10,4)");
            b.Property(x => x.RedemptionPercentage).HasColumnType("decimal(10,4)");
            b.Property(x => x.CurrentRedemptionValue).HasColumnType("decimal(18,2)");
            b.Property(x => x.PrizeDrawAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.Currency).IsRequired().HasMaxLength(3);
            b.Property(x => x.TotalPaidAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.MathematicalReserve).HasColumnType("decimal(18,2)");
            b.Property(x => x.SurrenderQuota).HasColumnType("decimal(10,4)");
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.BondNumber).IsUnique();
        });

        modelBuilder.Entity<CapitalizationPayment>(b =>
        {
            b.ToTable("CapitalizationPayments");
            b.HasKey(x => x.Id);
            b.Property(x => x.BondId).IsRequired();
            b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            b.Property(x => x.Currency).IsRequired().HasMaxLength(3);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(x => x.BondId);
            b.HasIndex(x => x.DueDate);
        });
    }
}
