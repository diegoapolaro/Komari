using Komari.Domain.Enums;

namespace Komari.Application.CashSessions.DTOs;

public record AddCashMovementRequest(
    CashMovementType Type,
    decimal Amount,
    string Description
);
