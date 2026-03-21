using Microsoft.EntityFrameworkCore;
using OpenFinance.PensionService.Domain.Entities;

namespace OpenFinance.PensionService.Infrastructure.Persistence;

public class PensionDbContext(DbContextOptions<PensionDbContext> options) : DbContext(options)
{
    public DbSet<Pension> Pensions => Set<Pension>();
    public DbSet<PensionBalance> PensionBalances => Set<PensionBalance>();
    public DbSet<PensionContribution> PensionContributions => Set<PensionContribution>();
    public DbSet<PensionWithdrawal> PensionWithdrawals => Set<PensionWithdrawal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pension>(b =>
        {
            b.ToTable("Pensions");
            b.HasKey(p => p.Id);
            b.Property(p => p.UserId).IsRequired().HasMaxLength(256);
            b.Property(p => p.Type).HasConversion<string>().HasMaxLength(20);
            b.Property(p => p.Modality).HasConversion<string>().HasMaxLength(20);
            b.Property(p => p.ProductName).IsRequired().HasMaxLength(256);
            b.Property(p => p.InsurerName).IsRequired().HasMaxLength(256);
            b.Property(p => p.InsurerCnpj).IsRequired().HasMaxLength(18);
            b.Property(p => p.Status).HasConversion<string>().HasMaxLength(30);
            b.Property(p => p.CertificateNumber).IsRequired().HasMaxLength(100);
            b.Property(p => p.TaxRegime).HasConversion<string>().HasMaxLength(20);
            b.Property(p => p.ContributionAmount).HasColumnType("decimal(18,2)");
            b.Property(p => p.ContributionFrequency).HasConversion<string>().HasMaxLength(20);
            b.Property(p => p.Currency).IsRequired().HasMaxLength(3);
            b.Property(p => p.BeneficiaryName).IsRequired().HasMaxLength(256);
            b.Property(p => p.ManagementFeeRate).HasColumnType("decimal(10,4)");
            b.Property(p => p.LoadingRate).HasColumnType("decimal(10,4)");
            b.Property(p => p.IncomeType).HasConversion<string>().HasMaxLength(30);
            b.HasIndex(p => p.UserId);
            b.HasIndex(p => p.CertificateNumber).IsUnique();
        });

        modelBuilder.Entity<PensionBalance>(b =>
        {
            b.ToTable("PensionBalances");
            b.HasKey(pb => pb.Id);
            b.Property(pb => pb.PensionId).IsRequired();
            b.Property(pb => pb.GrossBalance).HasColumnType("decimal(18,2)");
            b.Property(pb => pb.NetBalance).HasColumnType("decimal(18,2)");
            b.Property(pb => pb.TotalContributions).HasColumnType("decimal(18,2)");
            b.Property(pb => pb.TotalYield).HasColumnType("decimal(18,2)");
            b.Property(pb => pb.ManagementFee).HasColumnType("decimal(18,2)");
            b.Property(pb => pb.LoadingFee).HasColumnType("decimal(18,2)");
            b.Property(pb => pb.Currency).IsRequired().HasMaxLength(3);
            b.HasIndex(pb => pb.PensionId).IsUnique();
        });

        modelBuilder.Entity<PensionContribution>(b =>
        {
            b.ToTable("PensionContributions");
            b.HasKey(pc => pc.Id);
            b.Property(pc => pc.PensionId).IsRequired();
            b.Property(pc => pc.Amount).HasColumnType("decimal(18,2)");
            b.Property(pc => pc.Currency).IsRequired().HasMaxLength(3);
            b.Property(pc => pc.Type).HasConversion<string>().HasMaxLength(20);
            b.Property(pc => pc.Status).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(pc => pc.PensionId);
            b.HasIndex(pc => pc.ContributionDate);
        });

        modelBuilder.Entity<PensionWithdrawal>(b =>
        {
            b.ToTable("PensionWithdrawals");
            b.HasKey(pw => pw.Id);
            b.Property(pw => pw.PensionId).IsRequired();
            b.Property(pw => pw.GrossAmount).HasColumnType("decimal(18,2)");
            b.Property(pw => pw.TaxAmount).HasColumnType("decimal(18,2)");
            b.Property(pw => pw.NetAmount).HasColumnType("decimal(18,2)");
            b.Property(pw => pw.Currency).IsRequired().HasMaxLength(3);
            b.Property(pw => pw.Type).HasConversion<string>().HasMaxLength(30);
            b.HasIndex(pw => pw.PensionId);
            b.HasIndex(pw => pw.WithdrawalDate);
        });
    }
}
