using Komari.Domain.Common;

namespace Komari.Domain.Entities;

/// <summary>
/// Represents a menu item / product offered by the restaurant.
/// </summary>
public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public bool IsAvailable { get; private set; } = true;
    public string? ImageUrl { get; private set; }

    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;

    // Required by EF Core
    protected Product() { }

    public Product(string name, decimal price, Guid categoryId, string? description = null, string? imageUrl = null)
    {
        SetName(name);
        SetPrice(price);
        SetCategoryId(categoryId);
        Description = description?.Trim();
        ImageUrl = imageUrl?.Trim();
        IsAvailable = true;
    }

    public void UpdateDetails(string name, decimal price, Guid categoryId, string? description, string? imageUrl)
    {
        SetName(name);
        SetPrice(price);
        SetCategoryId(categoryId);
        Description = description?.Trim();
        ImageUrl = imageUrl?.Trim();
        TouchUpdated();
    }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
        TouchUpdated();
    }

    private void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        Name = name.Trim();
    }

    private void SetPrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        }
        Price = price;
    }

    private void SetCategoryId(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            throw new ArgumentException("CategoryId must be a valid non-empty Guid.", nameof(categoryId));
        }
        CategoryId = categoryId;
    }
}
