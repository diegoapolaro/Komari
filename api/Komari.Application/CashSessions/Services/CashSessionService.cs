using FluentValidation;
using Komari.Application.CashSessions.DTOs;
using Komari.Application.CashSessions.Interfaces;
using Komari.Application.Common.Exceptions;
using Komari.Domain.Entities;
using Komari.Domain.Enums;
using Komari.Domain.Repositories;

namespace Komari.Application.CashSessions.Services;

public class CashSessionService : ICashSessionService
{
    private readonly ICashSessionRepository _cashSessionRepository;
    private readonly IValidator<OpenCashSessionRequest> _openValidator;
    private readonly IValidator<CloseCashSessionRequest> _closeValidator;
    private readonly IValidator<AddCashMovementRequest> _movementValidator;

    public CashSessionService(
        ICashSessionRepository cashSessionRepository,
        IValidator<OpenCashSessionRequest> openValidator,
        IValidator<CloseCashSessionRequest> closeValidator,
        IValidator<AddCashMovementRequest> movementValidator)
    {
        _cashSessionRepository = cashSessionRepository;
        _openValidator = openValidator;
        _closeValidator = closeValidator;
        _movementValidator = movementValidator;
    }

    public async Task<IReadOnlyList<CashSessionResponse>> GetAllAsync(
        CashSessionStatus? status = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _cashSessionRepository.GetAllAsync(status, includeInactive, cancellationToken);
        return sessions.Select(MapToResponse).ToList();
    }

    public async Task<CashSessionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var session = await _cashSessionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(CashSession), id);

        return MapToResponse(session);
    }

    public async Task<CashSessionResponse> GetCurrentOpenAsync(CancellationToken cancellationToken = default)
    {
        var session = await _cashSessionRepository.GetCurrentOpenAsync(cancellationToken)
            ?? throw new NotFoundException("Não há sessão de caixa aberta no momento.");

        return MapToResponse(session);
    }

    public async Task<CashSessionResponse> OpenAsync(OpenCashSessionRequest request, CancellationToken cancellationToken = default)
    {
        await _openValidator.ValidateAndThrowAsync(request, cancellationToken);

        // Verifica se já existe uma sessão aberta
        var currentOpen = await _cashSessionRepository.GetCurrentOpenAsync(cancellationToken);
        if (currentOpen != null)
        {
            throw new ConflictException(
                $"Já existe uma sessão de caixa aberta (aberta em {currentOpen.OpenedAt:dd/MM/yyyy HH:mm}). Feche-a antes de abrir uma nova.");
        }

        var session = new CashSession(request.InitialAmount, request.OperatorName);
        await _cashSessionRepository.AddAsync(session, cancellationToken);

        // Recarrega para ter coleções inicializadas
        var created = await _cashSessionRepository.GetByIdAsync(session.Id, cancellationToken);
        return MapToResponse(created ?? session);
    }

    public async Task<CashMovementResponse> AddMovementAsync(
        Guid sessionId,
        AddCashMovementRequest request,
        CancellationToken cancellationToken = default)
    {
        await _movementValidator.ValidateAndThrowAsync(request, cancellationToken);

        var session = await _cashSessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new NotFoundException(nameof(CashSession), sessionId);

        CashMovement movement;

        if (request.Type == CashMovementType.Supply)
        {
            movement = session.AddSupply(request.Amount, request.Description);
        }
        else
        {
            movement = session.AddWithdrawal(request.Amount, request.Description);
        }

        await _cashSessionRepository.UpdateAsync(session, cancellationToken);

        return MapMovementToResponse(movement);
    }

    public async Task<CashSessionResponse> CloseAsync(
        Guid id,
        CloseCashSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        await _closeValidator.ValidateAndThrowAsync(request, cancellationToken);

        var session = await _cashSessionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(CashSession), id);

        session.Close(request.DeclaredAmount, request.Notes);
        await _cashSessionRepository.UpdateAsync(session, cancellationToken);

        return MapToResponse(session);
    }

    private static CashSessionResponse MapToResponse(CashSession session)
    {
        var movementResponses = session.Movements
            .Where(m => m.IsActive)
            .OrderByDescending(m => m.PerformedAt)
            .Select(MapMovementToResponse)
            .ToList();

        return new CashSessionResponse(
            session.Id,
            session.Status,
            session.InitialAmount,
            session.DeclaredAmount,
            session.ExpectedCashAmount,
            session.Variance,
            session.OperatorName,
            session.OpenedAt,
            session.ClosedAt,
            session.ClosingNotes,
            session.Payments.Count(p => p.IsActive),
            session.Movements.Count(m => m.IsActive),
            movementResponses,
            session.IsActive,
            session.CreatedAt,
            session.UpdatedAt
        );
    }

    private static CashMovementResponse MapMovementToResponse(CashMovement movement)
    {
        return new CashMovementResponse(
            movement.Id,
            movement.Type,
            movement.Amount,
            movement.Description,
            movement.PerformedAt,
            movement.IsActive,
            movement.CreatedAt
        );
    }
}
