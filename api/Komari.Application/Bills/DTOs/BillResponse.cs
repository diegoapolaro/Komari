using Komari.Domain.Enums;

namespace Komari.Application.Bills.DTOs;

/// <summary>
/// Representação de saída detalhada de uma comanda.
/// </summary>
public record BillResponse(
    Guid Id,
    int Number,
    Guid? TableId,
    int? TableNumber,
    string? CounterName,
    BillStatus Status,
    string? CustomerName,
    string? Notes,
    decimal TotalAmount,
    decimal TotalPaid,
    decimal RemainingBalance,
    int OrdersCount,
    int PaymentsCount,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
