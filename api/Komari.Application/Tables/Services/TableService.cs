using FluentValidation;
using Komari.Application.Common.Exceptions;
using Komari.Application.Tables.DTOs;
using Komari.Application.Tables.Interfaces;
using Komari.Domain.Entities;
using Komari.Domain.Enums;
using Komari.Domain.Repositories;

namespace Komari.Application.Tables.Services;

public class TableService : ITableService
{
    private readonly ITableRepository _tableRepository;
    private readonly IValidator<CreateTableRequest> _createValidator;
    private readonly IValidator<UpdateTableRequest> _updateValidator;

    public TableService(
        ITableRepository tableRepository,
        IValidator<CreateTableRequest> createValidator,
        IValidator<UpdateTableRequest> updateValidator)
    {
        _tableRepository = tableRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<TableResponse>> GetAllAsync(
        TableType? type = null,
        TableStatus? status = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var tables = await _tableRepository.GetAllAsync(type, status, includeInactive, cancellationToken);
        return tables.Select(MapToResponse).ToList();
    }

    public async Task<TableResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var table = await _tableRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Table), id);

        return MapToResponse(table);
    }

    public async Task<TableResponse> CreateAsync(CreateTableRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (await _tableRepository.ExistsByNumberAndTypeAsync(request.Number, request.Type, null, cancellationToken))
        {
            var typeName = request.Type == TableType.DiningTable ? "Mesa" : "Posição de Balcão";
            throw new ConflictException($"Já existe um(a) {typeName} ativo(a) com o número {request.Number}.");
        }

        var table = new Table(
            request.Number,
            request.Capacity,
            request.Type,
            request.Location
        );

        await _tableRepository.AddAsync(table, cancellationToken);

        return MapToResponse(table);
    }

    public async Task<TableResponse> UpdateAsync(Guid id, UpdateTableRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var table = await _tableRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Table), id);

        if (await _tableRepository.ExistsByNumberAndTypeAsync(request.Number, request.Type, id, cancellationToken))
        {
            var typeName = request.Type == TableType.DiningTable ? "Mesa" : "Posição de Balcão";
            throw new ConflictException($"Já existe outro(a) {typeName} ativo(a) com o número {request.Number}.");
        }

        table.UpdateDetails(
            request.Number,
            request.Capacity,
            request.Type,
            request.Location
        );

        await _tableRepository.UpdateAsync(table, cancellationToken);

        return MapToResponse(table);
    }

    public async Task UpdateStatusAsync(Guid id, TableStatus status, CancellationToken cancellationToken = default)
    {
        var table = await _tableRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Table), id);

        table.UpdateStatus(status);
        await _tableRepository.UpdateAsync(table, cancellationToken);
    }

    public async Task<IReadOnlyList<TableResponse>> InitializeTablesAsync(InitializeTablesRequest request, CancellationToken cancellationToken = default)
    {
        if (request.TotalTables <= 0 || request.TotalTables > 200)
        {
            throw new ValidationException("A quantidade de mesas deve ser entre 1 e 200.");
        }

        var capacity = request.DefaultCapacity > 0 ? request.DefaultCapacity : 4;
        var existingNumbers = await _tableRepository.GetExistingNumbersAsync(TableType.DiningTable, cancellationToken);
        var existingSet = new HashSet<int>(existingNumbers);

        var newTables = new List<Table>();
        for (int i = 1; i <= request.TotalTables; i++)
        {
            if (!existingSet.Contains(i))
            {
                newTables.Add(new Table(i, capacity, TableType.DiningTable, $"Mesa {i}"));
            }
        }

        if (newTables.Count > 0)
        {
            await _tableRepository.AddRangeAsync(newTables, cancellationToken);
        }

        var allTables = await _tableRepository.GetAllAsync(TableType.DiningTable, cancellationToken: cancellationToken);
        return allTables.Select(MapToResponse).ToList();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var table = await _tableRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Table), id);

        table.Deactivate();
        await _tableRepository.UpdateAsync(table, cancellationToken);
    }

    private static TableResponse MapToResponse(Table table) =>
        new(
            table.Id,
            table.Number,
            table.Capacity,
            table.Type,
            table.Status,
            table.Location,
            table.IsActive,
            table.CreatedAt
        );
}
