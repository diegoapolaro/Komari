using FluentValidation;
using Komari.Application.Common.Exceptions;
using Komari.Application.Products.DTOs;
using Komari.Application.Products.Interfaces;
using Komari.Domain.Entities;
using Komari.Domain.Repositories;

namespace Komari.Application.Products.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(Guid? categoryId = null, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(categoryId, includeInactive, cancellationToken);
        return products.Select(MapToResponse).ToList();
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        return MapToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        if (await _productRepository.ExistsByNameAsync(request.Name, null, cancellationToken))
        {
            throw new ConflictException($"Já existe um produto ativo com o nome '{request.Name}'.");
        }

        var product = new Product(
            request.Name,
            request.Price,
            request.CategoryId,
            request.Description,
            request.ImageUrl
        );

        await _productRepository.AddAsync(product, cancellationToken);

        // Retornamos com o nome da categoria já carregado
        return new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.IsAvailable,
            product.ImageUrl,
            product.CategoryId,
            category.Name,
            product.IsActive,
            product.CreatedAt
        );
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var product = await _productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        if (await _productRepository.ExistsByNameAsync(request.Name, id, cancellationToken))
        {
            throw new ConflictException($"Já existe outro produto ativo com o nome '{request.Name}'.");
        }

        product.UpdateDetails(
            request.Name,
            request.Price,
            request.CategoryId,
            request.Description,
            request.ImageUrl
        );

        await _productRepository.UpdateAsync(product, cancellationToken);

        return new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.IsAvailable,
            product.ImageUrl,
            product.CategoryId,
            category.Name,
            product.IsActive,
            product.CreatedAt
        );
    }

    public async Task SetAvailabilityAsync(Guid id, bool isAvailable, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        product.SetAvailability(isAvailable);
        await _productRepository.UpdateAsync(product, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        product.Deactivate();
        await _productRepository.UpdateAsync(product, cancellationToken);
    }

    private static ProductResponse MapToResponse(Product product) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.IsAvailable,
            product.ImageUrl,
            product.CategoryId,
            product.Category?.Name ?? string.Empty,
            product.IsActive,
            product.CreatedAt
        );
}
