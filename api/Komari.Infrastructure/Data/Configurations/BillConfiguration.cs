using Komari.Domain.Entities;
using Komari.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Komari.Infrastructure.Data.Configurations;

public class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.ToTable("bills");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Number)
            .IsRequired();

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(b => b.CounterName)
            .HasMaxLength(100);

        builder.Property(b => b.CustomerName)
            .HasMaxLength(100);

        builder.Property(b => b.Notes)
            .HasMaxLength(500);

        builder.Property(b => b.OpenedAt)
            .IsRequired();

        builder.Property(b => b.ClosedAt);

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.UpdatedAt);

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(b => b.Table)
            .WithMany()
            .HasForeignKey(b => b.TableId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.TableId);
        builder.HasIndex(b => new { b.Number, b.Status });
    }
}
