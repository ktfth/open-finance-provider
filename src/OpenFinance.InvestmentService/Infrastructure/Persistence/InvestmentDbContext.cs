using Microsoft.EntityFrameworkCore;
using OpenFinance.InvestmentService.Domain.Entities;

namespace OpenFinance.InvestmentService.Infrastructure.Persistence;

public class InvestmentDbContext(DbContextOptions<InvestmentDbContext> options) : DbContext(options)
{
    public DbSet<FixedIncomeInvestment> FixedIncomeInvestments => Set<FixedIncomeInvestment>();
    public DbSet<FixedIncomeBalance> FixedIncomeBalances => Set<FixedIncomeBalance>();
    public DbSet<FixedIncomeTransaction> FixedIncomeTransactions => Set<FixedIncomeTransaction>();
    public DbSet<VariableIncomeInvestment> VariableIncomeInvestments => Set<VariableIncomeInvestment>();
    public DbSet<VariableIncomeTransaction> VariableIncomeTransactions => Set<VariableIncomeTransaction>();
    public DbSet<TreasuryBondInvestment> TreasuryBondInvestments => Set<TreasuryBondInvestment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FixedIncomeInvestment>(b =>
        {
            b.ToTable("FixedIncomeInvestments");
            b.HasKey(i => i.Id);
            b.Property(i => i.UserId).IsRequired().HasMaxLength(256);
            b.Property(i => i.Type).HasConversion<string>().HasMaxLength(30);
            b.Property(i => i.ProductName).IsRequired().HasMaxLength(256);
            b.Property(i => i.Issuer).IsRequired().HasMaxLength(256);
            b.Property(i => i.ISIN).HasMaxLength(12);
            b.Property(i => i.FaceValue).HasColumnType("decimal(18,6)");
            b.Property(i => i.PurchaseUnitPrice).HasColumnType("decimal(18,6)");
            b.Property(i => i.Quantity).HasColumnType("decimal(18,6)");
            b.Property(i => i.Currency).IsRequired().HasMaxLength(3);
            b.Property(i => i.Indexer).HasConversion<string>().HasMaxLength(20);
            b.Property(i => i.IndexerPercentage).HasColumnType("decimal(10,4)");
            b.Property(i => i.PreFixedRate).HasColumnType("decimal(10,4)");
            b.Property(i => i.PostFixedRate).HasColumnType("decimal(10,4)");
            b.Property(i => i.TaxExemptionPercentage).HasColumnType("decimal(5,2)");
            b.Property(i => i.RemunerationType).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(i => i.UserId);
            b.HasIndex(i => i.ISIN);
        });

        modelBuilder.Entity<FixedIncomeBalance>(b =>
        {
            b.ToTable("FixedIncomeBalances");
            b.HasKey(bl => bl.Id);
            b.Property(bl => bl.InvestmentId).IsRequired();
            b.Property(bl => bl.GrossAmount).HasColumnType("decimal(18,2)");
            b.Property(bl => bl.IncomeTax).HasColumnType("decimal(18,2)");
            b.Property(bl => bl.IOFTax).HasColumnType("decimal(18,2)");
            b.Property(bl => bl.PurchaseUnitPrice).HasColumnType("decimal(18,6)");
            b.Property(bl => bl.UpdatedUnitPrice).HasColumnType("decimal(18,6)");
            b.Property(bl => bl.Quantity).HasColumnType("decimal(18,6)");
            b.Property(bl => bl.Currency).IsRequired().HasMaxLength(3);
            b.HasIndex(bl => bl.InvestmentId).IsUnique();
        });

        modelBuilder.Entity<FixedIncomeTransaction>(b =>
        {
            b.ToTable("FixedIncomeTransactions");
            b.HasKey(t => t.Id);
            b.Property(t => t.InvestmentId).IsRequired();
            b.Property(t => t.Type).HasConversion<string>().HasMaxLength(30);
            b.Property(t => t.Quantity).HasColumnType("decimal(18,6)");
            b.Property(t => t.UnitPrice).HasColumnType("decimal(18,6)");
            b.Property(t => t.GrossValue).HasColumnType("decimal(18,2)");
            b.Property(t => t.TaxValue).HasColumnType("decimal(18,2)");
            b.Property(t => t.Currency).IsRequired().HasMaxLength(3);
            b.HasIndex(t => t.InvestmentId);
            b.HasIndex(t => t.TransactionDate);
        });

        modelBuilder.Entity<VariableIncomeInvestment>(b =>
        {
            b.ToTable("VariableIncomeInvestments");
            b.HasKey(i => i.Id);
            b.Property(i => i.UserId).IsRequired().HasMaxLength(256);
            b.Property(i => i.Type).HasConversion<string>().HasMaxLength(30);
            b.Property(i => i.Ticker).IsRequired().HasMaxLength(10);
            b.Property(i => i.ProductName).IsRequired().HasMaxLength(256);
            b.Property(i => i.ISIN).HasMaxLength(12);
            b.Property(i => i.Quantity).HasColumnType("decimal(18,6)");
            b.Property(i => i.AveragePrice).HasColumnType("decimal(18,6)");
            b.Property(i => i.CurrentPrice).HasColumnType("decimal(18,6)");
            b.Property(i => i.Currency).IsRequired().HasMaxLength(3);
            b.HasIndex(i => i.UserId);
            b.HasIndex(i => i.Ticker);
        });

        modelBuilder.Entity<VariableIncomeTransaction>(b =>
        {
            b.ToTable("VariableIncomeTransactions");
            b.HasKey(t => t.Id);
            b.Property(t => t.InvestmentId).IsRequired();
            b.Property(t => t.Type).HasConversion<string>().HasMaxLength(30);
            b.Property(t => t.Quantity).HasColumnType("decimal(18,6)");
            b.Property(t => t.UnitPrice).HasColumnType("decimal(18,6)");
            b.Property(t => t.GrossValue).HasColumnType("decimal(18,2)");
            b.Property(t => t.BrokerageFee).HasColumnType("decimal(18,2)");
            b.Property(t => t.TaxValue).HasColumnType("decimal(18,2)");
            b.Property(t => t.Currency).IsRequired().HasMaxLength(3);
            b.HasIndex(t => t.InvestmentId);
            b.HasIndex(t => t.TransactionDate);
        });

        modelBuilder.Entity<TreasuryBondInvestment>(b =>
        {
            b.ToTable("TreasuryBondInvestments");
            b.HasKey(i => i.Id);
            b.Property(i => i.UserId).IsRequired().HasMaxLength(256);
            b.Property(i => i.ProductName).IsRequired().HasMaxLength(256);
            b.Property(i => i.BondType).HasConversion<string>().HasMaxLength(50);
            b.Property(i => i.NominalQuantity).HasColumnType("decimal(18,6)");
            b.Property(i => i.NominalUnitPrice).HasColumnType("decimal(18,6)");
            b.Property(i => i.UpdatedUnitPrice).HasColumnType("decimal(18,6)");
            b.Property(i => i.Currency).IsRequired().HasMaxLength(3);
            b.Property(i => i.RateType).HasColumnType("decimal(10,4)");
            b.Property(i => i.PurchaseIndexValue).HasColumnType("decimal(10,4)");
            b.HasIndex(i => i.UserId);
        });
    }
}
