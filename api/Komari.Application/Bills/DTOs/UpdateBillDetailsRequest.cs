namespace Komari.Application.Bills.DTOs;

/// <summary>
/// Dados para atualizar o cliente e/ou observações de uma comanda em andamento.
/// </summary>
/// <param name="CustomerName">Nome ou identificação do cliente.</param>
/// <param name="Notes">Observações do atendimento.</param>
public record UpdateBillDetailsRequest(
    string? CustomerName = null,
    string? Notes = null
);
