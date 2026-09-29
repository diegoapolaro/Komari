using Komari.Domain.Enums;

namespace Komari.Application.Payments.DTOs;

public record RegisterPaymentRequest(
    Guid BillId,
    PaymentMethod Method,
    decimal Amount,
    decimal? AmountTendered = null,
    string? Notes = null
);
