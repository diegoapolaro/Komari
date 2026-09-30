using Komari.Domain.Entities;
using Komari.Domain.Enums;

namespace Komari.Domain.Repositories;

public interface IDeliveryOrderRepository
{
    Task<DeliveryOrder?> GetByIdAsync(Guid id, bool includeOrder = true, CancellationToken cancellationToken = default);
    Task<DeliveryOrder?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeliveryOrder>> GetAllAsync(
        OrderStatus? status = null,
        Guid? customerId = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(DeliveryOrder deliveryOrder, CancellationToken cancellationToken = default);
    Task UpdateAsync(DeliveryOrder deliveryOrder, CancellationToken cancellationToken = default);
}
