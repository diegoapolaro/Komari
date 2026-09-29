using Komari.Domain.Common;

namespace Komari.Domain.Entities;

/// <summary>
/// Representa um item individual de consumo dentro de um pedido (Order).
/// Mantém um snapshot imutável do preço unitário praticado no momento do lançamento.
/// </summary>
public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Order? Order { get; private set; }

    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }

    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }
    public string? Notes { get; private set; }
    public string? Size { get; private set; }

    // Construtor protegido exigido pelo EF Core
    protected OrderItem() { }

    public OrderItem(
        Guid productId,
        decimal unitPrice,
        int quantity,
        string? notes = null,
        string? size = null)
    {
        SetProductId(productId);
        SetUnitPrice(unitPrice);
        SetQuantity(quantity);
        Notes = notes?.Trim();
        Size = size?.Trim();
        RecalculateTotalPrice();
    }

    internal void AttachToOrder(Guid orderId)
    {
        OrderId = orderId;
    }

    public void UpdateQuantity(int quantity)
    {
        SetQuantity(quantity);
        RecalculateTotalPrice();
        TouchUpdated();
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        TouchUpdated();
    }

    private void SetProductId(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("O identificador do produto (ProductId) não pode ser vazio.", nameof(productId));
        }
        ProductId = productId;
    }

    private void SetUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "O preço unitário não pode ser negativo.");
        }
        UnitPrice = unitPrice;
    }

    private void SetQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade do item deve ser maior que zero.");
        }
        Quantity = quantity;
    }

    private void RecalculateTotalPrice()
    {
        TotalPrice = Quantity * UnitPrice;
    }
}
