using Microsoft.EntityFrameworkCore;
using OpenFinance.InsuranceService.Domain.Entities;

namespace OpenFinance.InsuranceService.Infrastructure.Persistence;

public class InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : DbContext(options)
{
    public DbSet<Insurance> Insurances => Set<Insurance>();
    public DbSet<InsuranceClaim> InsuranceClaims => Set<InsuranceClaim>();
    public DbSet<InsuranceCoverage> InsuranceCoverages => Set<InsuranceCoverage>();
    public DbSet<InsurancePremiumPayment> InsurancePremiumPayments => Set<InsurancePremiumPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Insurance>(b =>
        {
            b.ToTable("Insurances");
            b.HasKey(i => i.Id);
            b.Property(i => i.UserId).IsRequired().HasMaxLength(256);
            b.Property(i => i.Type).HasConversion<string>().HasMaxLength(30);
            b.Property(i => i.ProductName).IsRequired().HasMaxLength(256);
            b.Property(i => i.InsurerName).IsRequired().HasMaxLength(256);
            b.Property(i => i.InsurerCnpj).IsRequired().HasMaxLength(18);
            b.Property(i => i.Status).HasConversion<string>().HasMaxLength(30);
            b.Property(i => i.PolicyNumber).IsRequired().HasMaxLength(100);
            b.Property(i => i.InsuredAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(i => i.PremiumAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(i => i.Currency).IsRequired().HasMaxLength(3);
            b.Property(i => i.InsuredCpfCnpj).IsRequired().HasMaxLength(18);
            b.Property(i => i.InsuredName).IsRequired().HasMaxLength(256);
            b.Property(i => i.BeneficiaryName).IsRequired().HasMaxLength(256);
            b.HasIndex(i => i.UserId);
            b.HasIndex(i => i.PolicyNumber).IsUnique();
        });

        modelBuilder.Entity<InsuranceClaim>(b =>
        {
            b.ToTable("InsuranceClaims");
            b.HasKey(c => c.Id);
            b.Property(c => c.InsuranceId).IsRequired();
            b.Property(c => c.ClaimNumber).IsRequired().HasMaxLength(100);
            b.Property(c => c.ClaimedAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(c => c.ApprovedAmount).HasColumnType("decimal(18,2)");
            b.Property(c => c.Currency).IsRequired().HasMaxLength(3);
            b.Property(c => c.Status).HasConversion<string>().HasMaxLength(30);
            b.HasIndex(c => c.InsuranceId);
            b.HasIndex(c => c.ClaimNumber).IsUnique();
        });

        modelBuilder.Entity<InsuranceCoverage>(b =>
        {
            b.ToTable("InsuranceCoverages");
            b.HasKey(c => c.Id);
            b.Property(c => c.InsuranceId).IsRequired();
            b.Property(c => c.CoverageName).IsRequired().HasMaxLength(256);
            b.Property(c => c.Type).HasConversion<string>().HasMaxLength(30);
            b.Property(c => c.InsuredAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(c => c.DeductibleAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(c => c.Currency).IsRequired().HasMaxLength(3);
            b.HasIndex(c => c.InsuranceId);
        });

        modelBuilder.Entity<InsurancePremiumPayment>(b =>
        {
            b.ToTable("InsurancePremiumPayments");
            b.HasKey(p => p.Id);
            b.Property(p => p.InsuranceId).IsRequired();
            b.Property(p => p.Amount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(p => p.Currency).IsRequired().HasMaxLength(3);
            b.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(p => p.InsuranceId);
            b.HasIndex(p => p.DueDate);
        });
    }
}
