using Komari.Domain.Common;
using Komari.Domain.Enums;

namespace Komari.Domain.Entities;

/// <summary>
/// Representa um pagamento parcial ou total vinculado a uma comanda (Bill)
/// e registrado na sessão de caixa (CashSession) ativa.
/// </summary>
public class Payment : BaseEntity
{
    public Guid BillId { get; private set; }
    public Bill? Bill { get; private set; }

    public Guid CashSessionId { get; private set; }
    public CashSession? CashSession { get; private set; }

    public PaymentMethod Method { get; private set; }
    public decimal Amount { get; private set; }
    public decimal? AmountTendered { get; private set; }
    public decimal ChangeGiven { get; private set; }
    public DateTime PaidAt { get; private set; }
    public string? Notes { get; private set; }

    protected Payment() { }

    public Payment(
        Guid billId,
        Guid cashSessionId,
        PaymentMethod method,
        decimal amount,
        decimal? amountTendered = null,
        string? notes = null)
    {
        SetBillId(billId);
        SetCashSessionId(cashSessionId);
        Method = method;
        SetAmount(amount);
        PaidAt = DateTime.UtcNow;
        Notes = notes?.Trim();

        if (method == PaymentMethod.Cash)
        {
            if (!amountTendered.HasValue || amountTendered.Value < amount)
            {
                throw new ArgumentException(
                    "Para pagamentos em dinheiro, o valor entregue (AmountTendered) deve ser informado e ser maior ou igual ao valor da parcela.",
                    nameof(amountTendered));
            }
            AmountTendered = amountTendered.Value;
            ChangeGiven = amountTendered.Value - amount;
        }
        else
        {
            AmountTendered = null;
            ChangeGiven = 0m;
        }
    }

    private void SetBillId(Guid billId)
    {
        if (billId == Guid.Empty)
            throw new ArgumentException("O identificador da comanda (BillId) não pode ser vazio.", nameof(billId));
        BillId = billId;
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
            throw new ArgumentOutOfRangeException(nameof(amount), "O valor do pagamento deve ser maior que zero.");
        Amount = amount;
    }
}
