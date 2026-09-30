using Komari.Domain.Enums;

namespace Komari.Application.Orders.DTOs;

/// <summary>
/// Representação de saída detalhada de um pedido contendo seus itens e valores calculados.
/// </summary>
public record OrderResponse(
    Guid Id,
    OrderType Type,
    Guid? BillId,
    int? BillNumber,
    Guid? TableId,
    int? TableNumber,
    string? CounterName,
    OrderStatus Status,
    decimal Total,
    string? Notes,
    string? CancellationReason,
    IReadOnlyList<OrderItemResponse> Items,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
