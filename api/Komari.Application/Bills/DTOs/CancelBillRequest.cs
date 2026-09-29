namespace Komari.Application.Bills.DTOs;

/// <summary>
/// Dados necessários para cancelamento de uma comanda sem faturamento.
/// </summary>
/// <param name="Reason">Motivo do cancelamento (obrigatório para auditoria).</param>
public record CancelBillRequest(
    string Reason
);
