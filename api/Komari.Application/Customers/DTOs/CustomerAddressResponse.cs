namespace Komari.Application.Customers.DTOs;

public record CustomerAddressResponse(
    Guid Id,
    Guid CustomerId,
    string Street,
    string Number,
    string Neighborhood,
    string? ZipCode,
    string? Complement,
    string? ReferencePoint,
    bool IsDefault,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
