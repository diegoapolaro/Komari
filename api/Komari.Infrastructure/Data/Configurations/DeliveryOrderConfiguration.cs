using Komari.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Komari.Infrastructure.Data.Configurations;

public class DeliveryOrderConfiguration : IEntityTypeConfiguration<DeliveryOrder>
{
    public void Configure(EntityTypeBuilder<DeliveryOrder> builder)
    {
        builder.ToTable("delivery_orders");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.OrderId)
            .IsRequired();

        builder.Property(d => d.CustomerId)
            .IsRequired();

        builder.Property(d => d.CustomerAddressId)
            .IsRequired();

        builder.Property(d => d.Street)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Number)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(d => d.Neighborhood)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.ZipCode)
            .HasMaxLength(10);

        builder.Property(d => d.Complement)
            .HasMaxLength(100);

        builder.Property(d => d.ReferencePoint)
            .HasMaxLength(200);

        builder.Property(d => d.DeliveryFee)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(d => d.Discount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(d => d.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(d => d.PaymentMethod)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(d => d.ChangeFor)
            .HasPrecision(18, 2);

        builder.Property(d => d.DriverName)
            .HasMaxLength(100);

        builder.Property(d => d.EstimatedMinutes);

        builder.Property(d => d.DispatchedAt);

        builder.Property(d => d.DeliveredAt);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.UpdatedAt);

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Relacionamento 1-para-1 com Order
        builder.HasOne(d => d.Order)
            .WithOne(o => o.DeliveryOrder)
            .HasForeignKey<DeliveryOrder>(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento com Customer e Address
        builder.HasOne(d => d.Customer)
            .WithMany()
            .HasForeignKey(d => d.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.CustomerAddress)
            .WithMany()
            .HasForeignKey(d => d.CustomerAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.OrderId)
            .IsUnique();

        builder.HasIndex(d => d.CustomerId);
        builder.HasIndex(d => d.CreatedAt);
    }
}
