namespace Komari.Application.Orders.DTOs;

/// <summary>
/// Dados para cancelamento de um pedido com justificativa obrigatória caso já esteja em produção.
/// </summary>
public record CancelOrderRequest(
    string Reason
);
