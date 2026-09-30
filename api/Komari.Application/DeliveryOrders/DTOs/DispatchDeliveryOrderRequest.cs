namespace Komari.Application.DeliveryOrders.DTOs;

/// <summary>
/// Dados para despacho de um pedido de delivery em trânsito com entregador/motoboy.
/// </summary>
public record DispatchDeliveryOrderRequest(
    string DriverName
);
