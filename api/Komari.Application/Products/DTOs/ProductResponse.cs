namespace Komari.Application.Products.DTOs;

public record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    bool IsAvailable,
    string? ImageUrl,
    Guid CategoryId,
    string CategoryName,
    bool IsActive,
    DateTime CreatedAt
);
