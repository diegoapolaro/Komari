namespace Komari.Application.Products.DTOs;

public record CreateProductRequest(
    string Name,
    decimal Price,
    Guid CategoryId,
    string? Description = null,
    string? ImageUrl = null
);
