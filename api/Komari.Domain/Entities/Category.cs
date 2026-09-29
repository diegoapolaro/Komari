using Komari.Domain.Common;

namespace Komari.Domain.Entities;

/// <summary>
/// Represents a menu category (e.g. Pizzas Tradicionais, Bebidas, Sobremesas).
/// </summary>
public class Category : BaseEntity
{
    private readonly List<Product> _products = [];

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }

    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    // Required by EF Core
    protected Category() { }

    public Category(string name, string? description = null, int displayOrder = 0)
    {
        SetName(name);
        Description = description?.Trim();
        DisplayOrder = displayOrder;
    }

    public void UpdateDetails(string name, string? description, int displayOrder)
    {
        SetName(name);
        Description = description?.Trim();
        DisplayOrder = displayOrder;
        TouchUpdated();
    }

    private void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        Name = name.Trim();
    }
}
