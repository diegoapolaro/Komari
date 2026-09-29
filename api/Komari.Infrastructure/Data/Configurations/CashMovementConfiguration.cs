using Komari.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Komari.Infrastructure.Data.Configurations;

public class CashMovementConfiguration : IEntityTypeConfiguration<CashMovement>
{
    public void Configure(EntityTypeBuilder<CashMovement> builder)
    {
        builder.ToTable("cash_movements");

        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Type)
            .IsRequired();

        builder.Property(cm => cm.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(cm => cm.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(cm => cm.PerformedAt)
            .IsRequired();

        builder.HasOne(cm => cm.CashSession)
            .WithMany(cs => cs.Movements)
            .HasForeignKey(cm => cm.CashSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cm => cm.CashSessionId)
            .HasDatabaseName("IX_cash_movements_cash_session_id");
    }
}
