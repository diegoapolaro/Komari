using Komari.Application.Payments.DTOs;
using Komari.Application.Payments.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Komari.Api.Controllers;

public class PaymentsController : ApiControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Registra um pagamento parcial ou total em uma comanda.
    /// Exige que haja uma sessão de caixa aberta.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentResponse>> Register(
        [FromBody] RegisterPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.RegisterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Obtém os detalhes de um pagamento pelo seu identificador único.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lista todos os pagamentos registrados em uma comanda específica.
    /// </summary>
    [HttpGet("by-bill/{billId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PaymentResponse>>> GetByBillId(
        Guid billId,
        CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetByBillIdAsync(billId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lista todos os pagamentos registrados em uma sessão de caixa específica.
    /// </summary>
    [HttpGet("by-session/{sessionId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PaymentResponse>>> GetByCashSessionId(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetByCashSessionIdAsync(sessionId, cancellationToken);
        return Ok(result);
    }
}
