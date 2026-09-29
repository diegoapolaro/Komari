namespace Komari.Application.Categories.DTOs;

public record CreateCategoryRequest(
    string Name,
    string? Description = null,
    int DisplayOrder = 0
);
