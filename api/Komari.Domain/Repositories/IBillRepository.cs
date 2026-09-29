using Komari.Domain.Entities;
using Komari.Domain.Enums;

namespace Komari.Domain.Repositories;

public interface IBillRepository
{
    Task<Bill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Bill?> GetActiveByNumberAsync(int number, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bill>> GetAllAsync(
        BillStatus? status = null,
        Guid? tableId = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveByNumberAsync(int number, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountActiveByTableIdAsync(Guid tableId, Guid? excludeBillId = null, CancellationToken cancellationToken = default);
    Task<int> GetNextNumberAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Bill bill, CancellationToken cancellationToken = default);
    Task UpdateAsync(Bill bill, CancellationToken cancellationToken = default);
}
