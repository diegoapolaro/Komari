namespace Komari.Application.Customers.DTOs;

public record CreateCustomerRequest(
    string Name,
    string? Phone = null,
    string? Email = null,
    string? Document = null,
    string? Notes = null
);
