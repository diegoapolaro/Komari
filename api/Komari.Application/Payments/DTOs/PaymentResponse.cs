using Komari.Domain.Enums;

namespace Komari.Application.Payments.DTOs;

public record PaymentResponse(
    Guid Id,
    Guid BillId,
    int BillNumber,
    Guid CashSessionId,
    PaymentMethod Method,
    decimal Amount,
    decimal? AmountTendered,
    decimal ChangeGiven,
    DateTime PaidAt,
    string? Notes,
    decimal BillTotalAmount,
    decimal BillTotalPaid,
    decimal BillRemainingBalance,
    bool IsActive,
    DateTime CreatedAt
);
