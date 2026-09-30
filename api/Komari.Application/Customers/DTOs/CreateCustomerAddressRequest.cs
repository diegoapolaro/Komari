namespace Komari.Application.Customers.DTOs;

public record CreateCustomerAddressRequest(
    string Street,
    string Number,
    string Neighborhood,
    string? ZipCode = null,
    string? Complement = null,
    string? ReferencePoint = null,
    bool IsDefault = false
);
