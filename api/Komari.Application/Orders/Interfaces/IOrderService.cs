using Komari.Application.Orders.DTOs;
using Komari.Domain.Enums;

namespace Komari.Application.Orders.Interfaces;

public interface IOrderService
{
    Task<IReadOnlyList<OrderResponse>> GetAllAsync(
        Guid? billId = null,
        OrderStatus? status = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<OrderResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderResponse>> GetByBillIdAsync(Guid billId, CancellationToken cancellationToken = default);

    Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);

    Task<OrderResponse> AddItemsAsync(Guid orderId, AddItemsToOrderRequest request, CancellationToken cancellationToken = default);

    Task<OrderResponse> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);

    Task<OrderResponse> CancelAsync(Guid id, CancelOrderRequest request, CancellationToken cancellationToken = default);
}
