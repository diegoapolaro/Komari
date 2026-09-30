using Komari.Application.DeliveryOrders.DTOs;
using Komari.Domain.Enums;

namespace Komari.Application.DeliveryOrders.Interfaces;

public interface IDeliveryOrderService
{
    Task<IReadOnlyList<DeliveryOrderResponse>> GetAllAsync(
        OrderStatus? status = null,
        Guid? customerId = null,
        CancellationToken cancellationToken = default);

    Task<DeliveryOrderResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DeliveryOrderResponse> CreateAsync(CreateDeliveryOrderRequest request, CancellationToken cancellationToken = default);

    Task<DeliveryOrderResponse> DispatchAsync(Guid id, DispatchDeliveryOrderRequest request, CancellationToken cancellationToken = default);

    Task<DeliveryOrderResponse> DeliverAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DeliveryOrderResponse> CancelAsync(Guid id, CancelDeliveryOrderRequest request, CancellationToken cancellationToken = default);

    Task<DeliveryOrderResponse> UpdateEstimatedMinutesAsync(Guid id, UpdateEstimatedMinutesRequest request, CancellationToken cancellationToken = default);
}
