using Komari.Application.Tables.DTOs;
using Komari.Domain.Enums;

namespace Komari.Application.Tables.Interfaces;

public interface ITableService
{
    Task<IReadOnlyList<TableResponse>> GetAllAsync(
        TableType? type = null,
        TableStatus? status = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<TableResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TableResponse> CreateAsync(CreateTableRequest request, CancellationToken cancellationToken = default);

    Task<TableResponse> UpdateAsync(Guid id, UpdateTableRequest request, CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(Guid id, TableStatus status, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TableResponse>> InitializeTablesAsync(InitializeTablesRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
