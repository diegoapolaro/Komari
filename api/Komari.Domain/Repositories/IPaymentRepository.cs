using Komari.Domain.Entities;

namespace Komari.Domain.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetByBillIdAsync(Guid billId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetByCashSessionIdAsync(Guid cashSessionId, CancellationToken cancellationToken = default);
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
}
