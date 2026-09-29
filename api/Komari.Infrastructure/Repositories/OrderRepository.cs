using Komari.Domain.Entities;
using Komari.Domain.Enums;
using Komari.Domain.Repositories;
using Komari.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Komari.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly KomariDbContext _context;

    public OrderRepository(KomariDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id, bool includeItems = true, CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = _context.Orders
            .Include(o => o.Bill)
                .ThenInclude(b => b!.Table);

        if (includeItems)
        {
            query = query
                .Include(o => o.Items.Where(i => i.IsActive))
                    .ThenInclude(i => i.Product);
        }

        return await query.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByBillIdAsync(Guid billId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Bill)
                .ThenInclude(b => b!.Table)
            .Include(o => o.Items.Where(i => i.IsActive))
                .ThenInclude(i => i.Product)
            .Where(o => o.BillId == billId && o.IsActive)
            .OrderByDescending(o => o.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(
        Guid? billId = null,
        OrderStatus? status = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = _context.Orders
            .Include(o => o.Bill)
                .ThenInclude(b => b!.Table)
            .Include(o => o.Items.Where(i => i.IsActive))
                .ThenInclude(i => i.Product)
            .AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(o => o.IsActive);
        }

        if (billId.HasValue)
        {
            query = query.Where(o => o.BillId == billId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(order);
        if (entry.State == EntityState.Detached)
        {
            _context.Orders.Update(order);
        }

        foreach (var item in order.Items)
        {
            var itemEntry = _context.Entry(item);
            if (itemEntry.State == EntityState.Detached)
            {
                await _context.OrderItems.AddAsync(item, cancellationToken);
            }
            else if (itemEntry.State == EntityState.Modified)
            {
                bool exists = await _context.OrderItems.AsNoTracking().AnyAsync(i => i.Id == item.Id, cancellationToken);
                if (!exists)
                {
                    itemEntry.State = EntityState.Added;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
