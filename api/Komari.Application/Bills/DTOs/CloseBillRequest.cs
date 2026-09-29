namespace Komari.Application.Bills.DTOs;

/// <summary>
/// Dados opcionais fornecidos no encerramento da comanda.
/// </summary>
/// <param name="Notes">Observação ou anotação final de fechamento.</param>
public record CloseBillRequest(
    string? Notes = null
);
