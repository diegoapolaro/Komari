using Komari.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Komari.Infrastructure.Data;

public class KomariDbContext : DbContext
{
    public KomariDbContext(DbContextOptions<KomariDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Table> Tables => Set<Table>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KomariDbContext).Assembly);
    }
}
