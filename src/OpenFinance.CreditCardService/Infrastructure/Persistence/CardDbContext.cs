using Microsoft.EntityFrameworkCore;
using OpenFinance.CreditCardService.Domain.Entities;

namespace OpenFinance.CreditCardService.Infrastructure.Persistence;

public class CardDbContext(DbContextOptions<CardDbContext> options) : DbContext(options)
{
    public DbSet<CardAccount> CardAccounts => Set<CardAccount>();
    public DbSet<CardLimit> CardLimits => Set<CardLimit>();
    public DbSet<CardBill> CardBills => Set<CardBill>();
    public DbSet<CardTransaction> CardTransactions => Set<CardTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CardAccount>(b =>
        {
            b.ToTable("CardAccounts");
            b.HasKey(a => a.Id);
            b.Property(a => a.UserId).IsRequired().HasMaxLength(256);
            b.Property(a => a.LastFourDigits).IsRequired().HasMaxLength(4);
            b.Property(a => a.Brand).HasConversion<string>().HasMaxLength(30);
            b.Property(a => a.CardType).HasConversion<string>().HasMaxLength(20);
            b.Property(a => a.NetworkType).HasConversion<string>().HasMaxLength(30);
            b.Property(a => a.HolderName).IsRequired().HasMaxLength(256);
            b.Property(a => a.HolderCpf).IsRequired().HasMaxLength(14);
            b.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(a => a.UserId);
        });

        modelBuilder.Entity<CardLimit>(b =>
        {
            b.ToTable("CardLimits");
            b.HasKey(l => l.Id);
            b.Property(l => l.CardAccountId).IsRequired();
            b.Property(l => l.LimitType).HasConversion<string>().HasMaxLength(20);
            b.Property(l => l.CreditLineLimitType).IsRequired().HasMaxLength(50);
            b.Property(l => l.ConsolidationType).IsRequired().HasMaxLength(50);
            b.Property(l => l.IdentificationNumber).IsRequired().HasMaxLength(50);
            b.Property(l => l.LineName).IsRequired().HasMaxLength(100);
            b.Property(l => l.LimitAmountTotal).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(l => l.UsedAmountTotal).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(l => l.Currency).IsRequired().HasMaxLength(3);
            b.HasIndex(l => l.CardAccountId);
        });

        modelBuilder.Entity<CardBill>(b =>
        {
            b.ToTable("CardBills");
            b.HasKey(bi => bi.Id);
            b.Property(bi => bi.CardAccountId).IsRequired();
            b.Property(bi => bi.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(bi => bi.MinimumPaymentAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(bi => bi.Currency).IsRequired().HasMaxLength(3);
            b.Property(bi => bi.Status).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(bi => bi.CardAccountId);
            b.HasIndex(bi => bi.DueDate);
        });

        modelBuilder.Entity<CardTransaction>(b =>
        {
            b.ToTable("CardTransactions");
            b.HasKey(t => t.Id);
            b.Property(t => t.CardAccountId).IsRequired();
            b.Property(t => t.BillId).IsRequired();
            b.Property(t => t.IdentificationNumber).IsRequired().HasMaxLength(100);
            b.Property(t => t.LineName).IsRequired().HasMaxLength(100);
            b.Property(t => t.TransactionName).IsRequired().HasMaxLength(256);
            b.Property(t => t.BillIdentification).IsRequired().HasMaxLength(100);
            b.Property(t => t.TransactionType).HasConversion<string>().HasMaxLength(30);
            b.Property(t => t.Amount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(t => t.Currency).IsRequired().HasMaxLength(3);
            b.Property(t => t.BillPostDate).HasColumnType("decimal(18,2)");
            b.Property(t => t.PayeeMCC).HasColumnType("decimal(10,0)");
            b.HasIndex(t => t.CardAccountId);
            b.HasIndex(t => t.BillId);
            b.HasIndex(t => t.TransactionDateTime);
        });
    }
}
