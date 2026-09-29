using Komari.Application.Payments.DTOs;

namespace Komari.Application.Payments.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponse> RegisterAsync(RegisterPaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentResponse>> GetByBillIdAsync(Guid billId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentResponse>> GetByCashSessionIdAsync(Guid cashSessionId, CancellationToken cancellationToken = default);
}
