using Komari.Application.Orders.DTOs;
using Komari.Domain.Enums;

namespace Komari.Application.DeliveryOrders.DTOs;

/// <summary>
/// Dados de entrada para abertura de um pedido de entrega em domicílio.
/// </summary>
public record CreateDeliveryOrderRequest(
    Guid CustomerId,
    Guid CustomerAddressId,
    decimal DeliveryFee,
    decimal Discount,
    DeliveryPaymentMethod PaymentMethod,
    decimal? ChangeFor,
    int? EstimatedMinutes,
    string? Notes,
    IReadOnlyList<CreateOrderItemRequest> Items
);
