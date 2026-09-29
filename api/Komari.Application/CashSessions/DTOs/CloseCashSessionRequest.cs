namespace Komari.Application.CashSessions.DTOs;

public record CloseCashSessionRequest(
    decimal DeclaredAmount,
    string? Notes = null
);
