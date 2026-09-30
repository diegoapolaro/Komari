namespace Komari.Domain.Enums;

/// <summary>
/// Define a modalidade ou canal de atendimento de um pedido.
/// </summary>
public enum OrderType
{
    /// <summary>
    /// Pedido lançado para consumo em mesa no salão.
    /// </summary>
    DineIn = 1,

    /// <summary>
    /// Pedido para consumo rápido ou retirada direta no balcão.
    /// </summary>
    Counter = 2,

    /// <summary>
    /// Pedido para entrega em domicílio (despacho com entregador/motoboy).
    /// </summary>
    Delivery = 3
}
