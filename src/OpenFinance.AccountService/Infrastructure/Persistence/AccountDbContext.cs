using Microsoft.EntityFrameworkCore;
using OpenFinance.AccountService.Domain.Entities;

namespace OpenFinance.AccountService.Infrastructure.Persistence;

public class AccountDbContext(DbContextOptions<AccountDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<AccountBalance> AccountBalances => Set<AccountBalance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountBalance>(b =>
        {
            b.ToTable("AccountBalances");
            b.HasKey(ab => ab.Id);
            b.Property(ab => ab.AccountId).IsRequired();
            b.Property(ab => ab.AvailableBalance).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(ab => ab.BlockedBalance).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(ab => ab.Currency).IsRequired().HasMaxLength(3);
            b.Property(ab => ab.UpdatedAt).IsRequired();
            b.HasIndex(ab => ab.AccountId).IsUnique();
        });

        modelBuilder.Entity<Account>(b =>
        {
            b.ToTable("Accounts");
            b.HasKey(a => a.Id);
            b.Property(a => a.UserId).IsRequired().HasMaxLength(256);
            b.Property(a => a.AccountNumber).IsRequired().HasMaxLength(20);
            b.Property(a => a.BranchCode).IsRequired().HasMaxLength(10);
            b.Property(a => a.Type).HasConversion<string>().HasMaxLength(20);
            b.Property(a => a.Currency).IsRequired().HasMaxLength(3);
            b.Property(a => a.OwnerName).IsRequired().HasMaxLength(256);
            b.Property(a => a.Cpf).IsRequired().HasMaxLength(14);
            b.HasIndex(a => a.UserId);
            b.HasIndex(a => a.AccountNumber).IsUnique();
        });

        modelBuilder.Entity<Transaction>(b =>
        {
            b.ToTable("Transactions");
            b.HasKey(t => t.Id);
            b.Property(t => t.AccountId).IsRequired();
            b.Property(t => t.Amount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(t => t.Currency).IsRequired().HasMaxLength(3);
            b.Property(t => t.Type).IsRequired().HasMaxLength(50);
            b.Property(t => t.Description).HasMaxLength(512);
            b.Property(t => t.CompletedDateTime).IsRequired();
            b.HasIndex(t => t.AccountId);
            b.HasIndex(t => t.CompletedDateTime);
        });
    }
}
