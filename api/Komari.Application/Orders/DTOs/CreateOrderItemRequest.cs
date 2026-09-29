namespace Komari.Application.Orders.DTOs;

/// <summary>
/// Dados para inclusão de um item de consumo em um pedido.
/// </summary>
public record CreateOrderItemRequest(
    Guid ProductId,
    int Quantity,
    string? Notes = null,
    string? Size = null
);
