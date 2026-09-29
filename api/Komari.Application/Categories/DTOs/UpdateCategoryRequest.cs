namespace Komari.Application.Categories.DTOs;

public record UpdateCategoryRequest(
    string Name,
    string? Description,
    int DisplayOrder
);
