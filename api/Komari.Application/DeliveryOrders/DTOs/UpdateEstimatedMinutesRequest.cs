namespace Komari.Application.DeliveryOrders.DTOs;

/// <summary>
/// Dados para atualização da previsão estimada de entrega em minutos.
/// </summary>
public record UpdateEstimatedMinutesRequest(
    int Minutes
);
