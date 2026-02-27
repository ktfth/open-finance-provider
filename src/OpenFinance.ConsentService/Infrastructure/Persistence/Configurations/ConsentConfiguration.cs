using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OpenFinance.ConsentService.Domain.Entities;

namespace OpenFinance.ConsentService.Infrastructure.Persistence.Configurations;

public class ConsentConfiguration : IEntityTypeConfiguration<Consent>
{
    public void Configure(EntityTypeBuilder<Consent> builder)
    {
        builder.ToTable("Consents");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ClientId).IsRequired().HasMaxLength(256);
        builder.Property(c => c.UserId).IsRequired().HasMaxLength(256);
        builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(32);
        builder.Property(c => c.ExpiresAt).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt);
        builder.Property(c => c.RedirectUri).HasMaxLength(2048);
        builder.Property(c => c.RejectionReason).HasMaxLength(512);

        // Map the backing field directly so EF Core can materialize it from DB
        var permissionsConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

        builder.Property<List<string>>("_permissions")
            .HasColumnName("Permissions")
            .HasConversion(permissionsConverter)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Ignore(c => c.Permissions);

        builder.HasIndex(c => c.ClientId);
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.Status);
    }
}
