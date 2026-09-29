namespace Komari.Application.Products.DTOs;

public record UpdateProductRequest(
    string Name,
    decimal Price,
    Guid CategoryId,
    string? Description = null,
    string? ImageUrl = null
);

public record UpdateProductAvailabilityRequest(
    bool IsAvailable
);
