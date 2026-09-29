namespace Komari.Domain.Enums;

/// <summary>
/// Representa o estado do pedido no fluxo de produção da cozinha e entrega.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Pedido criado e aguardando aceite / início pela cozinha.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Em processo de preparação/cocção na cozinha (KDS).
    /// </summary>
    InPreparation = 2,

    /// <summary>
    /// Pronto para ser servido na mesa, entregue no balcão ou despachado.
    /// </summary>
    Ready = 3,

    /// <summary>
    /// Em trânsito com motoboy/entregador (modalidade Delivery).
    /// </summary>
    OutForDelivery = 4,

    /// <summary>
    /// Entregue com sucesso ao cliente / mesa / balcão.
    /// </summary>
    Delivered = 5,

    /// <summary>
    /// Pedido cancelado (com justificativa se já estava em produção).
    /// </summary>
    Cancelled = 6
}
