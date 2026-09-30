using Komari.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Komari.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.BillId)
            .IsRequired(false);

        builder.Property(o => o.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(o => o.Total)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.Notes)
            .HasMaxLength(500);

        builder.Property(o => o.CancellationReason)
            .HasMaxLength(500);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt);

        builder.Property(o => o.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(o => o.Bill)
            .WithMany(b => b.Orders)
            .HasForeignKey(o => o.BillId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.DeliveryOrder)
            .WithOne(d => d.Order)
            .HasForeignKey<DeliveryOrder>(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Habilita acesso ao backing field _items
        builder.Navigation(o => o.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(o => o.BillId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.Type);
        builder.HasIndex(o => o.CreatedAt);
    }
}
