namespace Komari.Application.DeliveryOrders.DTOs;

/// <summary>
/// Dados para cancelamento de um pedido de delivery.
/// </summary>
public record CancelDeliveryOrderRequest(
    string Reason
);
