namespace Komari.Application.Bills.DTOs;

/// <summary>
/// Dados necessários para abrir uma nova comanda física ou avulsa.
/// </summary>
/// <param name="Number">Número identificador opcional da comanda física. Se não informado, gerado automaticamente.</param>
/// <param name="TableId">ID opcional da mesa de salão à qual a comanda será vinculada.</param>
/// <param name="CounterName">Nome descritivo opcional quando o atendimento for de balcão (ex: 'Carlos Chopp').</param>
/// <param name="CustomerName">Nome ou identificação opcional do cliente.</param>
/// <param name="Notes">Observações iniciais do atendimento.</param>
public record OpenBillRequest(
    int? Number = null,
    Guid? TableId = null,
    string? CounterName = null,
    string? CustomerName = null,
    string? Notes = null
);
