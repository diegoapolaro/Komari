using FluentValidation;
using Komari.Application.Common.Exceptions;
using Komari.Application.Payments.DTOs;
using Komari.Application.Payments.Interfaces;
using Komari.Domain.Entities;
using Komari.Domain.Enums;
using Komari.Domain.Repositories;

namespace Komari.Application.Payments.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBillRepository _billRepository;
    private readonly ICashSessionRepository _cashSessionRepository;
    private readonly IValidator<RegisterPaymentRequest> _registerValidator;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IBillRepository billRepository,
        ICashSessionRepository cashSessionRepository,
        IValidator<RegisterPaymentRequest> registerValidator)
    {
        _paymentRepository = paymentRepository;
        _billRepository = billRepository;
        _cashSessionRepository = cashSessionRepository;
        _registerValidator = registerValidator;
    }

    public async Task<PaymentResponse> RegisterAsync(RegisterPaymentRequest request, CancellationToken cancellationToken = default)
    {
        await _registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        // 1. Validar sessão de caixa aberta
        var session = await _cashSessionRepository.GetCurrentOpenAsync(cancellationToken)
            ?? throw new ConflictException("Não há sessão de caixa aberta. Abra uma sessão antes de registrar pagamentos.");

        // 2. Validar comanda
        var bill = await _billRepository.GetByIdAsync(request.BillId, cancellationToken)
            ?? throw new NotFoundException(nameof(Bill), request.BillId);

        if (bill.Status != BillStatus.Open && bill.Status != BillStatus.Closing)
        {
            throw new ConflictException(
                $"Não é possível registrar pagamento na comanda #{bill.Number}. Status atual: '{bill.Status}'. A comanda precisa estar aberta ou em conferência.");
        }

        // 3. Validar valor contra saldo devedor
        if (request.Amount > bill.RemainingBalance)
        {
            throw new ConflictException(
                $"O valor do pagamento (R$ {request.Amount:F2}) excede o saldo devedor da comanda #{bill.Number} (R$ {bill.RemainingBalance:F2}).");
        }

        // 4. Criar pagamento
        var payment = new Payment(
            request.BillId,
            session.Id,
            request.Method,
            request.Amount,
            request.AmountTendered,
            request.Notes
        );

        await _paymentRepository.AddAsync(payment, cancellationToken);

        // 5. Recarregar bill para calcular saldos atualizados
        bill = await _billRepository.GetByIdAsync(request.BillId, cancellationToken);

        return MapToResponse(payment, bill!);
    }

    public async Task<PaymentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Payment), id);

        var bill = await _billRepository.GetByIdAsync(payment.BillId, cancellationToken);
        return MapToResponse(payment, bill!);
    }

    public async Task<IReadOnlyList<PaymentResponse>> GetByBillIdAsync(Guid billId, CancellationToken cancellationToken = default)
    {
        var bill = await _billRepository.GetByIdAsync(billId, cancellationToken)
            ?? throw new NotFoundException(nameof(Bill), billId);

        var payments = await _paymentRepository.GetByBillIdAsync(billId, cancellationToken);
        return payments.Select(p => MapToResponse(p, bill)).ToList();
    }

    public async Task<IReadOnlyList<PaymentResponse>> GetByCashSessionIdAsync(Guid cashSessionId, CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetByCashSessionIdAsync(cashSessionId, cancellationToken);

        var result = new List<PaymentResponse>();
        foreach (var payment in payments)
        {
            var bill = payment.Bill ?? await _billRepository.GetByIdAsync(payment.BillId, cancellationToken);
            result.Add(MapToResponse(payment, bill!));
        }

        return result;
    }

    private static PaymentResponse MapToResponse(Payment payment, Bill bill)
    {
        return new PaymentResponse(
            payment.Id,
            payment.BillId,
            bill.Number,
            payment.CashSessionId,
            payment.Method,
            payment.Amount,
            payment.AmountTendered,
            payment.ChangeGiven,
            payment.PaidAt,
            payment.Notes,
            bill.TotalAmount,
            bill.TotalPaid,
            bill.RemainingBalance,
            payment.IsActive,
            payment.CreatedAt
        );
    }
}
