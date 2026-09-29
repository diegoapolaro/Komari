using Komari.Domain.Entities;
using Komari.Domain.Enums;

namespace Komari.Domain.Repositories;

public interface ICashSessionRepository
{
    Task<CashSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CashSession?> GetCurrentOpenAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CashSession>> GetAllAsync(CashSessionStatus? status = null, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task AddAsync(CashSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(CashSession session, CancellationToken cancellationToken = default);
}
