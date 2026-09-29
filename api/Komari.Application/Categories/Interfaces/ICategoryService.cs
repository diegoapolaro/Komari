using Komari.Application.Categories.DTOs;

namespace Komari.Application.Categories.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<CategoryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
