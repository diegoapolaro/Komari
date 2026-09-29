using Komari.Application.Products.DTOs;

namespace Komari.Application.Products.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<ProductResponse>> GetAllAsync(Guid? categoryId = null, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task SetAvailabilityAsync(Guid id, bool isAvailable, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
