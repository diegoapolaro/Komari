namespace Komari.Application.Tables.DTOs;

/// <summary>
/// Parâmetros para inicialização ou sincronização em lote de mesas do salão (1 a N).
/// </summary>
/// <param name="TotalTables">Quantidade total de mesas desejadas no salão (ex: 20 criará mesas de 1 a 20).</param>
/// <param name="DefaultCapacity">Capacidade padrão de lugares por mesa (mínimo 1).</param>
public record InitializeTablesRequest(
    int TotalTables,
    int DefaultCapacity = 4
);
