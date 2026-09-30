using Komari.Domain.Entities;
using Komari.Domain.Enums;
using Komari.Domain.Repositories;
using Komari.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Komari.Infrastructure.Repositories;

public class DeliveryOrderRepository : IDeliveryOrderRepository
{
    private readonly KomariDbContext _context;

    public DeliveryOrderRepository(KomariDbContext context)
    {
        _context = context;
    }

    public async Task<DeliveryOrder?> GetByIdAsync(Guid id, bool includeOrder = true, CancellationToken cancellationToken = default)
    {
        IQueryable<DeliveryOrder> query = _context.DeliveryOrders
            .Include(d => d.Customer)
            .Include(d => d.CustomerAddress);

        if (includeOrder)
        {
            query = query
                .Include(d => d.Order)
                    .ThenInclude(o => o!.Items.Where(i => i.IsActive))
                        .ThenInclude(i => i.Product);
        }

        return await query.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<DeliveryOrder?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryOrders
            .Include(d => d.Customer)
            .Include(d => d.CustomerAddress)
            .Include(d => d.Order)
                .ThenInclude(o => o!.Items.Where(i => i.IsActive))
                    .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(d => d.OrderId == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<DeliveryOrder>> GetAllAsync(
        OrderStatus? status = null,
        Guid? customerId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<DeliveryOrder> query = _context.DeliveryOrders
            .Include(d => d.Customer)
            .Include(d => d.CustomerAddress)
            .Include(d => d.Order)
                .ThenInclude(o => o!.Items.Where(i => i.IsActive))
                    .ThenInclude(i => i.Product)
            .Where(d => d.IsActive)
            .AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(d => d.Order != null && d.Order.Status == status.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(d => d.CustomerId == customerId.Value);
        }

        return await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DeliveryOrder deliveryOrder, CancellationToken cancellationToken = default)
    {
        await _context.DeliveryOrders.AddAsync(deliveryOrder, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DeliveryOrder deliveryOrder, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(deliveryOrder);
        if (entry.State == EntityState.Detached)
        {
            _context.DeliveryOrders.Update(deliveryOrder);
        }

        if (deliveryOrder.Order != null)
        {
            var orderEntry = _context.Entry(deliveryOrder.Order);
            if (orderEntry.State == EntityState.Detached)
            {
                _context.Orders.Update(deliveryOrder.Order);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
