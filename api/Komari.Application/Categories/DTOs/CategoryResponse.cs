namespace Komari.Application.Categories.DTOs;

public record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt
);
