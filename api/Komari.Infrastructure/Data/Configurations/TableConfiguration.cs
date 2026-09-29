using Komari.Domain.Entities;
using Komari.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Komari.Infrastructure.Data.Configurations;

public class TableConfiguration : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.ToTable("tables");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Number)
            .IsRequired();

        builder.Property(t => t.Capacity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(t => t.Location)
            .HasMaxLength(100);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(t => new { t.Number, t.Type });
        builder.HasIndex(t => t.Status);
    }
}
