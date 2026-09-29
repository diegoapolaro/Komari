using Komari.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Komari.Infrastructure.Data.Configurations;

public class CashSessionConfiguration : IEntityTypeConfiguration<CashSession>
{
    public void Configure(EntityTypeBuilder<CashSession> builder)
    {
        builder.ToTable("cash_sessions");

        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.Status)
            .IsRequired();

        builder.Property(cs => cs.InitialAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(cs => cs.DeclaredAmount)
            .HasPrecision(18, 2);

        builder.Property(cs => cs.ExpectedCashAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(cs => cs.Variance)
            .HasPrecision(18, 2);

        builder.Property(cs => cs.OperatorName)
            .HasMaxLength(200);

        builder.Property(cs => cs.ClosingNotes)
            .HasMaxLength(1000);

        builder.Property(cs => cs.OpenedAt)
            .IsRequired();

        builder.HasMany(cs => cs.Payments)
            .WithOne(p => p.CashSession)
            .HasForeignKey(p => p.CashSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(cs => cs.Movements)
            .WithOne(m => m.CashSession)
            .HasForeignKey(m => m.CashSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice para buscar rapidamente a sessão aberta
        builder.HasIndex(cs => cs.Status)
            .HasFilter("\"Status\" = 1")
            .HasDatabaseName("IX_cash_sessions_status_open");
    }
}
