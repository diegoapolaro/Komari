using Komari.Domain.Common;
using Komari.Domain.Enums;

namespace Komari.Domain.Entities;

/// <summary>
/// Representa uma sessão (turno) de caixa. Controla abertura com fundo de troco,
/// acumula pagamentos em dinheiro, suprimentos e sangrias, e fecha com conferência cega.
/// </summary>
public class CashSession : BaseEntity
{
    private readonly List<Payment> _payments = new();
    private readonly List<CashMovement> _movements = new();

    public CashSessionStatus Status { get; private set; } = CashSessionStatus.Open;
    public decimal InitialAmount { get; private set; }
    public decimal? DeclaredAmount { get; private set; }
    public decimal ExpectedCashAmount { get; private set; }
    public decimal? Variance { get; private set; }
    public string? OperatorName { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public string? ClosingNotes { get; private set; }

    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
    public IReadOnlyCollection<CashMovement> Movements => _movements.AsReadOnly();

    protected CashSession() { }

    public CashSession(decimal initialAmount, string? operatorName = null)
    {
        if (initialAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(initialAmount), "O fundo de troco inicial não pode ser negativo.");

        InitialAmount = initialAmount;
        ExpectedCashAmount = initialAmount;
        OperatorName = operatorName?.Trim();
        Status = CashSessionStatus.Open;
        OpenedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Registra um suprimento (aporte manual de dinheiro na gaveta).
    /// </summary>
    public CashMovement AddSupply(decimal amount, string description)
    {
        EnsureIsOpen();

        var movement = new CashMovement(Id, CashMovementType.Supply, amount, description);
        _movements.Add(movement);

        RecalculateExpectedCash();
        TouchUpdated();

        return movement;
    }

    /// <summary>
    /// Registra uma sangria (retirada manual de dinheiro da gaveta).
    /// Valida que o valor não excede o saldo de dinheiro disponível.
    /// </summary>
    public CashMovement AddWithdrawal(decimal amount, string description)
    {
        EnsureIsOpen();

        // Recalcula antes de validar para ter o valor mais atualizado
        RecalculateExpectedCash();

        if (amount > ExpectedCashAmount)
        {
            throw new InvalidOperationException(
                $"Não é possível realizar sangria de R$ {amount:F2}. O saldo de dinheiro disponível na gaveta é R$ {ExpectedCashAmount:F2}.");
        }

        var movement = new CashMovement(Id, CashMovementType.Withdrawal, amount, description);
        _movements.Add(movement);

        RecalculateExpectedCash();
        TouchUpdated();

        return movement;
    }

    /// <summary>
    /// Fecha a sessão de caixa com conferência cega.
    /// O operador declara quanto há na gaveta e o sistema calcula a divergência.
    /// </summary>
    public void Close(decimal declaredAmount, string? notes = null)
    {
        EnsureIsOpen();

        if (declaredAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(declaredAmount), "O valor declarado não pode ser negativo.");

        RecalculateExpectedCash();

        DeclaredAmount = declaredAmount;
        Variance = declaredAmount - ExpectedCashAmount;
        ClosingNotes = notes?.Trim();
        Status = CashSessionStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        TouchUpdated();
    }

    /// <summary>
    /// Recalcula o saldo esperado de dinheiro na gaveta.
    /// Fórmula: Fundo Inicial + Dinheiro Recebido + Suprimentos - Sangrias - Trocos Dados
    /// </summary>
    public void RecalculateExpectedCash()
    {
        var cashReceived = _payments
            .Where(p => p.IsActive && p.Method == PaymentMethod.Cash)
            .Sum(p => p.Amount);

        var changeGiven = _payments
            .Where(p => p.IsActive && p.Method == PaymentMethod.Cash)
            .Sum(p => p.ChangeGiven);

        var supplies = _movements
            .Where(m => m.IsActive && m.Type == CashMovementType.Supply)
            .Sum(m => m.Amount);

        var withdrawals = _movements
            .Where(m => m.IsActive && m.Type == CashMovementType.Withdrawal)
            .Sum(m => m.Amount);

        ExpectedCashAmount = InitialAmount + cashReceived + supplies - withdrawals - changeGiven;
    }

    private void EnsureIsOpen()
    {
        if (Status != CashSessionStatus.Open)
        {
            throw new InvalidOperationException("Esta sessão de caixa já foi encerrada.");
        }
    }
}
