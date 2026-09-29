using Komari.Domain.Common;
using Komari.Domain.Enums;

namespace Komari.Domain.Entities;

/// <summary>
/// Representa uma movimentação manual de dinheiro na sessão de caixa:
/// suprimento (aporte) ou sangria (retirada).
/// </summary>
public class CashMovement : BaseEntity
{
    public Guid CashSessionId { get; private set; }
    public CashSession? CashSession { get; private set; }

    public CashMovementType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime PerformedAt { get; private set; }

    protected CashMovement() { }

    public CashMovement(Guid cashSessionId, CashMovementType type, decimal amount, string description)
    {
        SetCashSessionId(cashSessionId);
        Type = type;
        SetAmount(amount);
        SetDescription(description);
        PerformedAt = DateTime.UtcNow;
    }

    private void SetCashSessionId(Guid cashSessionId)
    {
        if (cashSessionId == Guid.Empty)
            throw new ArgumentException("O identificador da sessão de caixa (CashSessionId) não pode ser vazio.", nameof(cashSessionId));
        CashSessionId = cashSessionId;
    }

    private void SetAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "O valor da movimentação deve ser maior que zero.");
        Amount = amount;
    }

    private void SetDescription(string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));
        Description = description.Trim();
    }
}
