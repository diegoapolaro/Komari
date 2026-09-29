using Komari.Domain.Enums;

namespace Komari.Application.Orders.DTOs;

/// <summary>
/// Dados para alteração do status de um pedido no fluxo de produção da cozinha.
/// </summary>
public record UpdateOrderStatusRequest(
    OrderStatus Status,
    string? CancellationReason = null
);
