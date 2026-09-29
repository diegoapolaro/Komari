using Komari.Application.CashSessions.DTOs;
using Komari.Domain.Enums;

namespace Komari.Application.CashSessions.Interfaces;

public interface ICashSessionService
{
    Task<IReadOnlyList<CashSessionResponse>> GetAllAsync(CashSessionStatus? status = null, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<CashSessionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CashSessionResponse> GetCurrentOpenAsync(CancellationToken cancellationToken = default);
    Task<CashSessionResponse> OpenAsync(OpenCashSessionRequest request, CancellationToken cancellationToken = default);
    Task<CashMovementResponse> AddMovementAsync(Guid sessionId, AddCashMovementRequest request, CancellationToken cancellationToken = default);
    Task<CashSessionResponse> CloseAsync(Guid id, CloseCashSessionRequest request, CancellationToken cancellationToken = default);
}
