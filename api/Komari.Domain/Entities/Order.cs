using Komari.Domain.Common;
using Komari.Domain.Enums;

namespace Komari.Domain.Entities;

/// <summary>
/// Representa um pedido lançado para uma comanda (mesa ou balcão).
/// Atua como agregado raiz para os itens de consumo (OrderItem) e gerencia o fluxo na cozinha.
/// </summary>
public class Order : BaseEntity
{
    private readonly List<OrderItem> _items = new();

    public Guid BillId { get; private set; }
    public Bill? Bill { get; private set; }

    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal Total { get; private set; }
    public string? Notes { get; private set; }
    public string? CancellationReason { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // Construtor protegido exigido pelo EF Core
    protected Order() { }

    public Order(Guid billId, string? notes = null)
    {
        SetBillId(billId);
        Notes = notes?.Trim();
        Status = OrderStatus.Pending;
        Total = 0m;
    }

    public OrderItem AddItem(
        Guid productId,
        decimal unitPrice,
        int quantity,
        string? notes = null,
        string? size = null)
    {
        EnsureCanModifyItems();

        var item = new OrderItem(productId, unitPrice, quantity, notes, size);
        item.AttachToOrder(Id);
        _items.Add(item);

        RecalculateTotal();
        TouchUpdated();

        return item;
    }

    public void RemoveItem(Guid orderItemId)
    {
        EnsureCanModifyItems();

        var item = _items.FirstOrDefault(i => i.Id == orderItemId);
        if (item == null)
        {
            throw new InvalidOperationException($"Item com identificador '{orderItemId}' não encontrado neste pedido.");
        }

        _items.Remove(item);
        RecalculateTotal();
        TouchUpdated();
    }

    public void UpdateItemQuantity(Guid orderItemId, int newQuantity)
    {
        EnsureCanModifyItems();

        var item = _items.FirstOrDefault(i => i.Id == orderItemId);
        if (item == null)
        {
            throw new InvalidOperationException($"Item com identificador '{orderItemId}' não encontrado neste pedido.");
        }

        item.UpdateQuantity(newQuantity);
        RecalculateTotal();
        TouchUpdated();
    }

    public void RecalculateTotal()
    {
        Total = _items.Where(i => i.IsActive).Sum(i => i.TotalPrice);
    }

    public void UpdateStatus(OrderStatus newStatus, string? cancellationReason = null)
    {
        if (Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Não é possível alterar o status de um pedido que já foi cancelado.");
        }

        if (Status == OrderStatus.Delivered)
        {
            throw new InvalidOperationException("Pedidos já entregues não podem ter seu status alterado diretamente.");
        }

        if (newStatus == OrderStatus.Cancelled)
        {
            Cancel(cancellationReason ?? string.Empty);
            return;
        }

        Status = newStatus;
        TouchUpdated();
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Delivered)
        {
            throw new InvalidOperationException("Pedidos já entregues não podem ser cancelados diretamente.");
        }

        if (Status == OrderStatus.InPreparation || Status == OrderStatus.Ready)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("É obrigatório informar uma justificativa para cancelar um pedido em preparação ou pronto.", nameof(reason));
            }
        }

        Status = OrderStatus.Cancelled;
        CancellationReason = reason?.Trim();
        TouchUpdated();
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        TouchUpdated();
    }

    private void SetBillId(Guid billId)
    {
        if (billId == Guid.Empty)
        {
            throw new ArgumentException("O identificador da comanda (BillId) não pode ser vazio.", nameof(billId));
        }
        BillId = billId;
    }

    private void EnsureCanModifyItems()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException($"Não é possível alterar itens de um pedido que já está em status '{Status}'. Apenas pedidos 'Pending' aceitam modificação direta de itens.");
        }
    }
}
