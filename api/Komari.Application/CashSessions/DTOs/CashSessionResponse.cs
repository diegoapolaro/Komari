using Komari.Domain.Enums;

namespace Komari.Application.CashSessions.DTOs;

public record CashSessionResponse(
    Guid Id,
    CashSessionStatus Status,
    decimal InitialAmount,
    decimal? DeclaredAmount,
    decimal ExpectedCashAmount,
    decimal? Variance,
    string? OperatorName,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    string? ClosingNotes,
    int TotalPayments,
    int TotalMovements,
    IReadOnlyList<CashMovementResponse> Movements,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
