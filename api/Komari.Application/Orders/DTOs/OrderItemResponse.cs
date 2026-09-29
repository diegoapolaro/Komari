namespace Komari.Application.Orders.DTOs;

/// <summary>
/// Representação de saída detalhada de um item de pedido.
/// </summary>
public record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string? Notes,
    string? Size,
    bool IsActive,
    DateTime CreatedAt
);
