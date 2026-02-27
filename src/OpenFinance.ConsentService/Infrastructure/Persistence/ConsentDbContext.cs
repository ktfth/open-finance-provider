using Microsoft.EntityFrameworkCore;
using OpenFinance.ConsentService.Domain.Entities;
using OpenFinance.ConsentService.Infrastructure.Persistence.Configurations;

namespace OpenFinance.ConsentService.Infrastructure.Persistence;

public class ConsentDbContext(DbContextOptions<ConsentDbContext> options) : DbContext(options)
{
    public DbSet<Consent> Consents => Set<Consent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ConsentConfiguration());
    }
}
