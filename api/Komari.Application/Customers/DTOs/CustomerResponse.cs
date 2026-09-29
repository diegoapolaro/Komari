namespace Komari.Application.Customers.DTOs;

public record CustomerResponse(
    Guid Id,
    string Name,
    string? Phone,
    string? Email,
    string? Document,
    string? Notes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
