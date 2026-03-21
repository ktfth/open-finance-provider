using Microsoft.EntityFrameworkCore;
using OpenFinance.LoanService.Domain.Entities;

namespace OpenFinance.LoanService.Infrastructure.Persistence;

public class LoanDbContext(DbContextOptions<LoanDbContext> options) : DbContext(options)
{
    public DbSet<LoanContract> LoanContracts => Set<LoanContract>();
    public DbSet<LoanPayment> LoanPayments => Set<LoanPayment>();
    public DbSet<LoanInstalment> LoanInstalments => Set<LoanInstalment>();
    public DbSet<LoanWarranty> LoanWarranties => Set<LoanWarranty>();
    public DbSet<FinancingContract> FinancingContracts => Set<FinancingContract>();
    public DbSet<OverdraftContract> OverdraftContracts => Set<OverdraftContract>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoanContract>(b =>
        {
            b.ToTable("LoanContracts");
            b.HasKey(l => l.Id);
            b.Property(l => l.UserId).IsRequired().HasMaxLength(256);
            b.Property(l => l.ContractNumber).IsRequired().HasMaxLength(50);
            b.Property(l => l.Type).HasConversion<string>().HasMaxLength(50);
            b.Property(l => l.ProductName).IsRequired().HasMaxLength(256);
            b.Property(l => l.CompanyCnpj).IsRequired().HasMaxLength(18);
            b.Property(l => l.Status).HasConversion<string>().HasMaxLength(30);
            b.Property(l => l.ContractAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(l => l.OutstandingBalance).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(l => l.InterestRate).IsRequired().HasColumnType("decimal(10,6)");
            b.Property(l => l.InterestRateType).HasConversion<string>().HasMaxLength(20);
            b.Property(l => l.Indexer).HasConversion<string>().HasMaxLength(20);
            b.Property(l => l.Currency).IsRequired().HasMaxLength(3);
            b.Property(l => l.CET).IsRequired().HasColumnType("decimal(10,6)");
            b.Property(l => l.AmortizationType).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(l => l.UserId);
            b.HasIndex(l => l.ContractNumber).IsUnique();
        });

        modelBuilder.Entity<LoanPayment>(b =>
        {
            b.ToTable("LoanPayments");
            b.HasKey(p => p.Id);
            b.Property(p => p.ContractId).IsRequired();
            b.Property(p => p.PaidAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(p => p.PrincipalAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(p => p.InterestAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(p => p.FeesAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(p => p.ChargesAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(p => p.Currency).IsRequired().HasMaxLength(3);
            b.HasIndex(p => p.ContractId);
        });

        modelBuilder.Entity<LoanInstalment>(b =>
        {
            b.ToTable("LoanInstalments");
            b.HasKey(i => i.Id);
            b.Property(i => i.ContractId).IsRequired();
            b.Property(i => i.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(i => i.PrincipalAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(i => i.InterestAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(i => i.FeesAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(i => i.Currency).IsRequired().HasMaxLength(3);
            b.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(i => i.ContractId);
        });

        modelBuilder.Entity<LoanWarranty>(b =>
        {
            b.ToTable("LoanWarranties");
            b.HasKey(w => w.Id);
            b.Property(w => w.ContractId).IsRequired();
            b.Property(w => w.Type).HasConversion<string>().HasMaxLength(30);
            b.Property(w => w.WarrantySubType).IsRequired().HasMaxLength(100);
            b.Property(w => w.Currency).IsRequired().HasMaxLength(3);
            b.Property(w => w.Amount).IsRequired().HasColumnType("decimal(18,2)");
            b.HasIndex(w => w.ContractId);
        });

        modelBuilder.Entity<FinancingContract>(b =>
        {
            b.ToTable("FinancingContracts");
            b.HasKey(f => f.Id);
            b.Property(f => f.UserId).IsRequired().HasMaxLength(256);
            b.Property(f => f.ContractNumber).IsRequired().HasMaxLength(50);
            b.Property(f => f.FinancingType).HasConversion<string>().HasMaxLength(50);
            b.Property(f => f.ProductName).IsRequired().HasMaxLength(256);
            b.Property(f => f.CompanyCnpj).IsRequired().HasMaxLength(18);
            b.Property(f => f.Status).HasConversion<string>().HasMaxLength(30);
            b.Property(f => f.ContractAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(f => f.OutstandingBalance).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(f => f.InterestRate).IsRequired().HasColumnType("decimal(10,6)");
            b.Property(f => f.InterestRateType).HasConversion<string>().HasMaxLength(20);
            b.Property(f => f.Indexer).HasConversion<string>().HasMaxLength(20);
            b.Property(f => f.Currency).IsRequired().HasMaxLength(3);
            b.Property(f => f.CET).IsRequired().HasColumnType("decimal(10,6)");
            b.Property(f => f.AmortizationType).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(f => f.UserId);
            b.HasIndex(f => f.ContractNumber).IsUnique();
        });

        modelBuilder.Entity<OverdraftContract>(b =>
        {
            b.ToTable("OverdraftContracts");
            b.HasKey(o => o.Id);
            b.Property(o => o.UserId).IsRequired().HasMaxLength(256);
            b.Property(o => o.ContractNumber).IsRequired().HasMaxLength(50);
            b.Property(o => o.CompanyCnpj).IsRequired().HasMaxLength(18);
            b.Property(o => o.Status).HasConversion<string>().HasMaxLength(30);
            b.Property(o => o.ContractAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(o => o.OutstandingBalance).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(o => o.InterestRate).IsRequired().HasColumnType("decimal(10,6)");
            b.Property(o => o.InterestRateType).HasConversion<string>().HasMaxLength(20);
            b.Property(o => o.Indexer).HasConversion<string>().HasMaxLength(20);
            b.Property(o => o.Currency).IsRequired().HasMaxLength(3);
            b.HasIndex(o => o.UserId);
            b.HasIndex(o => o.ContractNumber).IsUnique();
        });
    }
}
