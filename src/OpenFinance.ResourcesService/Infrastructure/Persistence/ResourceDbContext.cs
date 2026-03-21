using Microsoft.EntityFrameworkCore;
using OpenFinance.ResourcesService.Domain.Entities;

namespace OpenFinance.ResourcesService.Infrastructure.Persistence;

public class ResourceDbContext(DbContextOptions<ResourceDbContext> options) : DbContext(options)
{
    public DbSet<Resource> Resources => Set<Resource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>(b =>
        {
            b.ToTable("Resources");
            b.HasKey(r => r.Id);
            b.Property(r => r.ConsentId).IsRequired();
            b.Property(r => r.ResourceId).IsRequired();
            b.Property(r => r.Type).HasConversion<string>().HasMaxLength(50);
            b.Property(r => r.Status).HasConversion<string>().HasMaxLength(50);
            b.HasIndex(r => r.ConsentId);
        });
    }
}
