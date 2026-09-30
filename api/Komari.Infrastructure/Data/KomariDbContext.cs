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
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<CashSession> CashSessions => Set<CashSession>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<CashMovement> CashMovements => Set<CashMovement>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<DeliveryOrder> DeliveryOrders => Set<DeliveryOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KomariDbContext).Assembly);
    }
}
