using Microsoft.EntityFrameworkCore;
using OpenFinance.CustomerService.Domain.Entities;

namespace OpenFinance.CustomerService.Infrastructure.Persistence;

public class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : DbContext(options)
{
    public DbSet<PersonalCustomer> PersonalCustomers => Set<PersonalCustomer>();
    public DbSet<PersonalQualification> PersonalQualifications => Set<PersonalQualification>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();
    public DbSet<BusinessCustomer> BusinessCustomers => Set<BusinessCustomer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersonalCustomer>(b =>
        {
            b.ToTable("PersonalCustomers");
            b.HasKey(c => c.Id);
            b.Property(c => c.UserId).IsRequired().HasMaxLength(256);
            b.Property(c => c.CpfNumber).IsRequired().HasMaxLength(14);
            b.Property(c => c.SocialName).IsRequired().HasMaxLength(256);
            b.Property(c => c.BirthDate).IsRequired().HasMaxLength(10);
            b.Property(c => c.MaritalStatus).HasConversion<string>().HasMaxLength(20);
            b.Property(c => c.Sex).HasConversion<string>().HasMaxLength(10);
            b.Property(c => c.Nationality).IsRequired().HasMaxLength(100);
            b.Property(c => c.BirthCountry).IsRequired().HasMaxLength(100);
            b.HasIndex(c => c.UserId).IsUnique();
            b.HasIndex(c => c.CpfNumber).IsUnique();
        });

        modelBuilder.Entity<PersonalQualification>(b =>
        {
            b.ToTable("PersonalQualifications");
            b.HasKey(q => q.Id);
            b.Property(q => q.UserId).IsRequired().HasMaxLength(256);
            b.Property(q => q.OccupationType).HasConversion<string>().HasMaxLength(30);
            b.Property(q => q.OccupationDescription).IsRequired().HasMaxLength(512);
            b.Property(q => q.InformedIncomeFrequency).HasConversion<string>().HasMaxLength(20);
            b.Property(q => q.InformedIncomeAmount).IsRequired().HasColumnType("decimal(18,2)");
            b.Property(q => q.InformedIncomeCurrency).IsRequired().HasMaxLength(3);
            b.Property(q => q.InformedIncomeDate).IsRequired();
            b.HasIndex(q => q.UserId).IsUnique();
        });

        modelBuilder.Entity<CustomerAddress>(b =>
        {
            b.ToTable("CustomerAddresses");
            b.HasKey(a => a.Id);
            b.Property(a => a.UserId).IsRequired().HasMaxLength(256);
            b.Property(a => a.Street).IsRequired().HasMaxLength(256);
            b.Property(a => a.Number).IsRequired().HasMaxLength(20);
            b.Property(a => a.Complement).HasMaxLength(100);
            b.Property(a => a.District).IsRequired().HasMaxLength(100);
            b.Property(a => a.City).IsRequired().HasMaxLength(100);
            b.Property(a => a.State).IsRequired().HasMaxLength(50);
            b.Property(a => a.PostalCode).IsRequired().HasMaxLength(10);
            b.Property(a => a.Country).IsRequired().HasMaxLength(100);
            b.Property(a => a.AddressType).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(a => a.UserId);
        });

        modelBuilder.Entity<CustomerContact>(b =>
        {
            b.ToTable("CustomerContacts");
            b.HasKey(c => c.Id);
            b.Property(c => c.UserId).IsRequired().HasMaxLength(256);
            b.Property(c => c.PhoneType).HasConversion<string>().HasMaxLength(20);
            b.Property(c => c.CountryCallingCode).IsRequired().HasMaxLength(5);
            b.Property(c => c.AreaCode).IsRequired().HasMaxLength(5);
            b.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(20);
            b.Property(c => c.Email).IsRequired().HasMaxLength(256);
            b.Property(c => c.IsMainEmail).IsRequired();
            b.HasIndex(c => c.UserId);
        });

        modelBuilder.Entity<BusinessCustomer>(b =>
        {
            b.ToTable("BusinessCustomers");
            b.HasKey(c => c.Id);
            b.Property(c => c.UserId).IsRequired().HasMaxLength(256);
            b.Property(c => c.CnpjNumber).IsRequired().HasMaxLength(18);
            b.Property(c => c.CompanyName).IsRequired().HasMaxLength(256);
            b.Property(c => c.TradeName).IsRequired().HasMaxLength(256);
            b.Property(c => c.IncorporationDate).IsRequired();
            b.HasIndex(c => c.UserId).IsUnique();
            b.HasIndex(c => c.CnpjNumber).IsUnique();
        });
    }
}
