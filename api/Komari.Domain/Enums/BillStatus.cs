namespace Komari.Domain.Enums;

/// <summary>
/// Representa os estados possíveis de uma comanda no estabelecimento.
/// </summary>
public enum BillStatus
{
    /// <summary>
    /// Comanda aberta e em consumo.
    /// </summary>
    Open = 1,

    /// <summary>
    /// Comanda finalizada e quitada.
    /// </summary>
    Closed = 2,

    /// <summary>
    /// Comanda cancelada (ex: aberta por engano, sem consumo).
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Comanda em fase de conferência / conta solicitada (aguardando pagamento).
    /// Não permite inserção de novos itens.
    /// </summary>
    Closing = 4
}
