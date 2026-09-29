using FluentValidation;
using Komari.Application.Categories.DTOs;
using Komari.Application.Categories.Interfaces;
using Komari.Application.Common.Exceptions;
using Komari.Domain.Entities;
using Komari.Domain.Repositories;

namespace Komari.Application.Categories.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IValidator<CreateCategoryRequest> _createValidator;
    private readonly IValidator<UpdateCategoryRequest> _updateValidator;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IValidator<CreateCategoryRequest> createValidator,
        IValidator<UpdateCategoryRequest> updateValidator)
    {
        _categoryRepository = categoryRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllAsync(includeInactive, cancellationToken);
        return categories.Select(MapToResponse).ToList();
    }

    public async Task<CategoryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), id);

        return MapToResponse(category);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (await _categoryRepository.ExistsByNameAsync(request.Name, null, cancellationToken))
        {
            throw new ConflictException($"Já existe uma categoria ativa com o nome '{request.Name}'.");
        }

        var category = new Category(request.Name, request.Description, request.DisplayOrder);
        await _categoryRepository.AddAsync(category, cancellationToken);

        return MapToResponse(category);
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), id);

        if (await _categoryRepository.ExistsByNameAsync(request.Name, id, cancellationToken))
        {
            throw new ConflictException($"Já existe outra categoria ativa com o nome '{request.Name}'.");
        }

        category.UpdateDetails(request.Name, request.Description, request.DisplayOrder);
        await _categoryRepository.UpdateAsync(category, cancellationToken);

        return MapToResponse(category);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), id);

        category.Deactivate();
        await _categoryRepository.UpdateAsync(category, cancellationToken);
    }

    private static CategoryResponse MapToResponse(Category category) =>
        new(
            category.Id,
            category.Name,
            category.Description,
            category.DisplayOrder,
            category.IsActive,
            category.CreatedAt
        );
}
