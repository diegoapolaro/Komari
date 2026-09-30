using Komari.Application.Orders.DTOs;
using Komari.Domain.Enums;

namespace Komari.Application.DeliveryOrders.DTOs;

/// <summary>
/// Representação de saída detalhada de um pedido de delivery, contendo cliente, snapshot de endereço,
/// totais calculados, meio de recebimento e rastreamento de despacho.
/// </summary>
public record DeliveryOrderResponse(
    Guid Id,
    Guid OrderId,
    Guid CustomerId,
    string CustomerName,
    string? CustomerPhone,
    Guid CustomerAddressId,
    string Street,
    string Number,
    string Neighborhood,
    string? ZipCode,
    string? Complement,
    string? ReferencePoint,
    decimal ItemsTotal,
    decimal DeliveryFee,
    decimal Discount,
    decimal TotalAmount,
    DeliveryPaymentMethod PaymentMethod,
    decimal? ChangeFor,
    string? DriverName,
    int? EstimatedMinutes,
    OrderStatus Status,
    string? Notes,
    string? CancellationReason,
    IReadOnlyList<OrderItemResponse> Items,
    DateTime? DispatchedAt,
    DateTime? DeliveredAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
