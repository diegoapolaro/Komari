using Komari.Domain.Enums;

namespace Komari.Application.CashSessions.DTOs;

public record CashMovementResponse(
    Guid Id,
    CashMovementType Type,
    decimal Amount,
    string Description,
    DateTime PerformedAt,
    bool IsActive,
    DateTime CreatedAt
);
