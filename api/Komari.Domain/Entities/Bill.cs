using Komari.Domain.Common;
using Komari.Domain.Enums;

namespace Komari.Domain.Entities;

/// <summary>
/// Representa uma comanda de consumo identificada por um número sequencial físico.
/// Pode ser vinculada a uma mesa de salão ou operar de forma avulsa (balcão/retirada).
/// </summary>
public class Bill : BaseEntity
{
    private readonly List<Order> _orders = new();

    public int Number { get; private set; }
    public Guid? TableId { get; private set; }
    public Table? Table { get; private set; }
    public string? CounterName { get; private set; }
    public BillStatus Status { get; private set; } = BillStatus.Open;
    public string? CustomerName { get; private set; }
    public string? Notes { get; private set; }
    public DateTime OpenedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; private set; }

    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    /// <summary>
    /// Calcula o valor total consumido na comanda em tempo real, somando os pedidos não cancelados.
    /// </summary>
    public decimal TotalAmount => _orders
        .Where(o => o.IsActive && o.Status != OrderStatus.Cancelled)
        .Sum(o => o.Total);

    // Construtor protegido exigido pelo EF Core
    protected Bill() { }

    public Bill(
        int number,
        Guid? tableId = null,
        string? counterName = null,
        string? customerName = null,
        string? notes = null)
    {
        SetNumber(number);
        TableId = tableId;
        CounterName = counterName?.Trim();
        CustomerName = customerName?.Trim();
        Notes = notes?.Trim();
        Status = BillStatus.Open;
        OpenedAt = DateTime.UtcNow;
    }

    public void RequestClosing()
    {
        if (Status != BillStatus.Open)
        {
            throw new InvalidOperationException($"Não é possível solicitar fechamento de comanda que está com o status '{Status}'.");
        }

        Status = BillStatus.Closing;
        TouchUpdated();
    }

    public void Reopen()
    {
        if (Status != BillStatus.Closing)
        {
            throw new InvalidOperationException($"Apenas comandas em conferência ('Closing') podem ser reabertas.");
        }

        Status = BillStatus.Open;
        TouchUpdated();
    }

    public bool HasPendingOrInPreparationOrders() =>
        _orders.Any(o => o.IsActive && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.InPreparation));

    public void Close(string? notes = null)
    {
        if (Status != BillStatus.Open && Status != BillStatus.Closing)
        {
            throw new InvalidOperationException($"Não é possível encerrar uma comanda que está com o status '{Status}'.");
        }

        if (HasPendingOrInPreparationOrders())
        {
            throw new InvalidOperationException("Não é possível encerrar uma comanda com pedidos pendentes ou em preparação na cozinha.");
        }

        Status = BillStatus.Closed;
        ClosedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(notes))
        {
            Notes = string.IsNullOrWhiteSpace(Notes) ? notes.Trim() : $"{Notes} | {notes.Trim()}";
        }

        TouchUpdated();
    }

    public void UpdateCustomer(string? customerName)
    {
        CustomerName = customerName?.Trim();
        TouchUpdated();
    }

    public void Cancel(string reason)
    {
        if (Status != BillStatus.Open)
        {
            throw new InvalidOperationException($"Não é possível cancelar uma comanda que está com o status '{Status}'.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("O motivo do cancelamento é obrigatório.", nameof(reason));
        }

        Status = BillStatus.Cancelled;
        ClosedAt = DateTime.UtcNow;
        Notes = string.IsNullOrWhiteSpace(Notes) ? $"Cancelada: {reason.Trim()}" : $"{Notes} | Cancelada: {reason.Trim()}";
        TouchUpdated();
    }

    public void AssignToTable(Guid tableId)
    {
        if (Status != BillStatus.Open)
        {
            throw new InvalidOperationException("Não é possível alterar a mesa de uma comanda que não está aberta.");
        }

        TableId = tableId;
        TouchUpdated();
    }

    public void UnassignTable()
    {
        if (Status != BillStatus.Open)
        {
            throw new InvalidOperationException("Não é possível desvincular a mesa de uma comanda que não está aberta.");
        }

        TableId = null;
        TouchUpdated();
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        TouchUpdated();
    }

    private void SetNumber(int number)
    {
        if (number <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "O número identificador da comanda deve ser maior que zero.");
        }
        Number = number;
    }
}
