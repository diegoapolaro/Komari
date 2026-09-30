namespace Komari.Application.Customers.DTOs;

public record UpdateCustomerAddressRequest(
    string Street,
    string Number,
    string Neighborhood,
    string? ZipCode = null,
    string? Complement = null,
    string? ReferencePoint = null,
    bool IsDefault = false
);
