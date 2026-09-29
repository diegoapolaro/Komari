namespace Komari.Domain.Enums;

/// <summary>
/// Tipo de movimentação manual no caixa.
/// </summary>
public enum CashMovementType
{
    /// <summary>Suprimento: aporte manual de dinheiro na gaveta.</summary>
    Supply = 1,
    /// <summary>Sangria: retirada manual de dinheiro da gaveta.</summary>
    Withdrawal = 2
}
