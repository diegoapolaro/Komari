using Komari.Domain.Common;
using Komari.Domain.Enums;

namespace Komari.Domain.Entities;

/// <summary>
/// Representa uma mesa de salão ou ponto de atendimento no balcão da pizzaria/restaurante.
/// </summary>
public class Table : BaseEntity
{
    public int Number { get; private set; }
    public int Capacity { get; private set; }
    public TableType Type { get; private set; }
    public TableStatus Status { get; private set; } = TableStatus.Available;
    public string? Location { get; private set; }

    // Construtor protegido exigido pelo EF Core para materialização de entidades
    protected Table() { }

    public Table(int number, int capacity, TableType type, string? location = null)
    {
        SetNumber(number);
        SetCapacity(capacity);
        Type = type;
        Location = location?.Trim();
        Status = TableStatus.Available;
    }

    public void UpdateDetails(int number, int capacity, TableType type, string? location)
    {
        SetNumber(number);
        SetCapacity(capacity);
        Type = type;
        Location = location?.Trim();
        TouchUpdated();
    }

    public void UpdateStatus(TableStatus status)
    {
        Status = status;
        TouchUpdated();
    }

    private void SetNumber(int number)
    {
        if (number <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "O número da mesa deve ser maior que zero.");
        }
        Number = number;
    }

    private void SetCapacity(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "A capacidade da mesa deve ser de pelo menos 1 lugar.");
        }
        Capacity = capacity;
    }
}
