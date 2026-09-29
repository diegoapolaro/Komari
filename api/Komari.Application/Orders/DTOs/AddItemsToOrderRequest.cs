namespace Komari.Application.Orders.DTOs;

/// <summary>
/// Dados para adicionar itens a um pedido pendente existente.
/// </summary>
public record AddItemsToOrderRequest(
    List<CreateOrderItemRequest> Items
);
