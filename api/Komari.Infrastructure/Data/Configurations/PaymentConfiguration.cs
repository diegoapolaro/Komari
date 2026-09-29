using Komari.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Komari.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Method)
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.AmountTendered)
            .HasPrecision(18, 2);

        builder.Property(p => p.ChangeGiven)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.PaidAt)
            .IsRequired();

        builder.Property(p => p.Notes)
            .HasMaxLength(500);

        builder.HasOne(p => p.Bill)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CashSession)
            .WithMany(cs => cs.Payments)
            .HasForeignKey(p => p.CashSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.BillId)
            .HasDatabaseName("IX_payments_bill_id");

        builder.HasIndex(p => p.CashSessionId)
            .HasDatabaseName("IX_payments_cash_session_id");
    }
}
