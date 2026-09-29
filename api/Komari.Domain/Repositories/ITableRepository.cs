using Komari.Domain.Entities;
using Komari.Domain.Enums;

namespace Komari.Domain.Repositories;

public interface ITableRepository
{
    Task<Table?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Table>> GetAllAsync(TableType? type = null, TableStatus? status = null, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNumberAndTypeAsync(int number, TableType type, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetExistingNumbersAsync(TableType type, CancellationToken cancellationToken = default);
    Task AddAsync(Table table, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Table> tables, CancellationToken cancellationToken = default);
    Task UpdateAsync(Table table, CancellationToken cancellationToken = default);
}
