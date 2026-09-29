namespace Komari.Application.CashSessions.DTOs;

public record OpenCashSessionRequest(
    decimal InitialAmount,
    string? OperatorName = null
);
