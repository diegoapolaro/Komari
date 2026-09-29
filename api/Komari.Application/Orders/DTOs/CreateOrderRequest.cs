namespace Komari.Application.Orders.DTOs;

/// <summary>
/// Dados para abertura e envio de um pedido para a comanda (mesa ou balcão).
/// </summary>
public record CreateOrderRequest(
    Guid BillId,
    List<CreateOrderItemRequest> Items,
    string? Notes = null
);
