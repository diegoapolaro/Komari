namespace Komari.Domain.Enums;

/// <summary>
/// Define a forma de pagamento combinada para o pedido de delivery (pago antecipadamente ou na entrega).
/// </summary>
public enum DeliveryPaymentMethod
{
    /// <summary>
    /// Pagamento em dinheiro físico na entrega (com ou sem necessidade de troco).
    /// </summary>
    Cash = 1,

    /// <summary>
    /// Pagamento via PIX (chave dinâmica/QR Code antecipado ou chave no ato da entrega).
    /// </summary>
    Pix = 2,

    /// <summary>
    /// Cartão de Crédito na maquininha portátil do entregador.
    /// </summary>
    CreditCard = 3,

    /// <summary>
    /// Cartão de Débito na maquininha portátil do entregador.
    /// </summary>
    DebitCard = 4
}
