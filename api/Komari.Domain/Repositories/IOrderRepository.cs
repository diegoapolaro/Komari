using Komari.Domain.Entities;
using Komari.Domain.Enums;

namespace Komari.Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, bool includeItems = true, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByBillIdAsync(Guid billId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetAllAsync(
        Guid? billId = null,
        OrderStatus? status = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
}
