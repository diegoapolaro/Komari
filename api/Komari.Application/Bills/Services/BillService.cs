using FluentValidation;
using Komari.Application.Bills.DTOs;
using Komari.Application.Bills.Interfaces;
using Komari.Application.Common.Exceptions;
using Komari.Domain.Entities;
using Komari.Domain.Enums;
using Komari.Domain.Repositories;

namespace Komari.Application.Bills.Services;

public class BillService : IBillService
{
    private readonly IBillRepository _billRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IValidator<OpenBillRequest> _openValidator;
    private readonly IValidator<CancelBillRequest> _cancelValidator;

    public BillService(
        IBillRepository billRepository,
        ITableRepository tableRepository,
        IValidator<OpenBillRequest> openValidator,
        IValidator<CancelBillRequest> cancelValidator)
    {
        _billRepository = billRepository;
        _tableRepository = tableRepository;
        _openValidator = openValidator;
        _cancelValidator = cancelValidator;
    }

    public async Task<IReadOnlyList<BillResponse>> GetAllAsync(
        BillStatus? status = null,
        Guid? tableId = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var bills = await _billRepository.GetAllAsync(status, tableId, includeInactive, cancellationToken);
        return bills.Select(b => MapToResponse(b, b.Table?.Number)).ToList();
    }

    public async Task<BillResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bill = await _billRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Bill), id);

        return MapToResponse(bill, bill.Table?.Number);
    }

    public async Task<BillResponse> GetActiveByNumberAsync(int number, CancellationToken cancellationToken = default)
    {
        var bill = await _billRepository.GetActiveByNumberAsync(number, cancellationToken)
            ?? throw new NotFoundException($"Comanda ativa com número {number} não encontrada.");

        return MapToResponse(bill, bill.Table?.Number);
    }

    public async Task<BillResponse> OpenAsync(OpenBillRequest request, CancellationToken cancellationToken = default)
    {
        await _openValidator.ValidateAndThrowAsync(request, cancellationToken);

        int billNumber = request.Number.HasValue && request.Number.Value > 0
            ? request.Number.Value
            : await _billRepository.GetNextNumberAsync(cancellationToken);

        if (await _billRepository.ExistsActiveByNumberAsync(billNumber, null, cancellationToken))
        {
            throw new ConflictException($"Já existe uma comanda aberta com o número {billNumber}.");
        }

        Table? table = null;

        if (request.TableId.HasValue)
        {
            table = await _tableRepository.GetByIdAsync(request.TableId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Table), request.TableId.Value);

            if (!table.IsActive)
            {
                throw new ConflictException("Não é possível abrir comanda para uma mesa inativa.");
            }

            // Ao vincular comanda à mesa, altera automaticamente seu status para Ocupada
            if (table.Status != TableStatus.Occupied)
            {
                table.UpdateStatus(TableStatus.Occupied);
                await _tableRepository.UpdateAsync(table, cancellationToken);
            }
        }

        var bill = new Bill(
            billNumber,
            request.TableId,
            request.CounterName,
            request.CustomerName,
            request.Notes
        );

        await _billRepository.AddAsync(bill, cancellationToken);

        return MapToResponse(bill, table?.Number);
    }

    public async Task<BillResponse> RequestClosingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bill = await _billRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Bill), id);

        bill.RequestClosing();
        await _billRepository.UpdateAsync(bill, cancellationToken);

        if (bill.TableId.HasValue)
        {
            var table = await _tableRepository.GetByIdAsync(bill.TableId.Value, cancellationToken);
            if (table != null && table.Status == TableStatus.Occupied)
            {
                table.UpdateStatus(TableStatus.Closing);
                await _tableRepository.UpdateAsync(table, cancellationToken);
            }
        }

        return MapToResponse(bill, bill.Table?.Number);
    }

    public async Task<BillResponse> ReopenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bill = await _billRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Bill), id);

        bill.Reopen();
        await _billRepository.UpdateAsync(bill, cancellationToken);

        if (bill.TableId.HasValue)
        {
            var table = await _tableRepository.GetByIdAsync(bill.TableId.Value, cancellationToken);
            if (table != null && table.Status == TableStatus.Closing)
            {
                table.UpdateStatus(TableStatus.Occupied);
                await _tableRepository.UpdateAsync(table, cancellationToken);
            }
        }

        return MapToResponse(bill, bill.Table?.Number);
    }

    public async Task<BillResponse> UpdateDetailsAsync(Guid id, UpdateBillDetailsRequest request, CancellationToken cancellationToken = default)
    {
        var bill = await _billRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Bill), id);

        if (bill.Status != BillStatus.Open && bill.Status != BillStatus.Closing)
        {
            throw new InvalidOperationException($"Não é possível alterar observações de uma comanda encerrada ou cancelada.");
        }

        if (request.CustomerName != null)
        {
            bill.UpdateCustomer(request.CustomerName);
        }

        if (request.Notes != null)
        {
            bill.UpdateNotes(request.Notes);
        }

        await _billRepository.UpdateAsync(bill, cancellationToken);

        return MapToResponse(bill, bill.Table?.Number);
    }

    public async Task<BillResponse> CloseAsync(Guid id, CloseBillRequest? request = null, CancellationToken cancellationToken = default)
    {
        var bill = await _billRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Bill), id);

        bill.Close(request?.Notes);
        await _billRepository.UpdateAsync(bill, cancellationToken);

        if (bill.TableId.HasValue)
        {
            await ReleaseTableIfNoActiveBillsRemainingAsync(bill.TableId.Value, bill.Id, cancellationToken);
        }

        return MapToResponse(bill, bill.Table?.Number);
    }

    public async Task<BillResponse> CancelAsync(Guid id, CancelBillRequest request, CancellationToken cancellationToken = default)
    {
        await _cancelValidator.ValidateAndThrowAsync(request, cancellationToken);

        var bill = await _billRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Bill), id);

        bill.Cancel(request.Reason);
        await _billRepository.UpdateAsync(bill, cancellationToken);

        if (bill.TableId.HasValue)
        {
            await ReleaseTableIfNoActiveBillsRemainingAsync(bill.TableId.Value, bill.Id, cancellationToken);
        }

        return MapToResponse(bill, bill.Table?.Number);
    }

    private async Task ReleaseTableIfNoActiveBillsRemainingAsync(Guid tableId, Guid currentBillId, CancellationToken cancellationToken)
    {
        var remainingActiveBills = await _billRepository.CountActiveByTableIdAsync(tableId, currentBillId, cancellationToken);

        if (remainingActiveBills == 0)
        {
            var table = await _tableRepository.GetByIdAsync(tableId, cancellationToken);
            if (table != null && (table.Status == TableStatus.Occupied || table.Status == TableStatus.Closing))
            {
                table.UpdateStatus(TableStatus.Available);
                await _tableRepository.UpdateAsync(table, cancellationToken);
            }
        }
    }

    private static BillResponse MapToResponse(Bill bill, int? tableNumber)
    {
        return new BillResponse(
            bill.Id,
            bill.Number,
            bill.TableId,
            tableNumber,
            bill.CounterName,
            bill.Status,
            bill.CustomerName,
            bill.Notes,
            bill.TotalAmount,
            bill.Orders.Count(o => o.IsActive),
            bill.OpenedAt,
            bill.ClosedAt,
            bill.IsActive,
            bill.CreatedAt,
            bill.UpdatedAt
        );
    }
}
