using Komari.Application.Bills.DTOs;
using Komari.Domain.Enums;

namespace Komari.Application.Bills.Interfaces;

public interface IBillService
{
    Task<IReadOnlyList<BillResponse>> GetAllAsync(
        BillStatus? status = null,
        Guid? tableId = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<BillResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<BillResponse> GetActiveByNumberAsync(int number, CancellationToken cancellationToken = default);

    Task<BillResponse> OpenAsync(OpenBillRequest request, CancellationToken cancellationToken = default);

    Task<BillResponse> RequestClosingAsync(Guid id, CancellationToken cancellationToken = default);

    Task<BillResponse> ReopenAsync(Guid id, CancellationToken cancellationToken = default);

    Task<BillResponse> UpdateDetailsAsync(Guid id, UpdateBillDetailsRequest request, CancellationToken cancellationToken = default);

    Task<BillResponse> CloseAsync(Guid id, CloseBillRequest? request = null, CancellationToken cancellationToken = default);

    Task<BillResponse> CancelAsync(Guid id, CancelBillRequest request, CancellationToken cancellationToken = default);
}
