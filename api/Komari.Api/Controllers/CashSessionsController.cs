using Komari.Application.CashSessions.DTOs;
using Komari.Application.CashSessions.Interfaces;
using Komari.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Komari.Api.Controllers;

public class CashSessionsController : ApiControllerBase
{
    private readonly ICashSessionService _cashSessionService;

    public CashSessionsController(ICashSessionService cashSessionService)
    {
        _cashSessionService = cashSessionService;
    }

    /// <summary>
    /// Lista as sessões de caixa com filtro opcional por status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CashSessionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CashSessionResponse>>> GetAll(
        [FromQuery] CashSessionStatus? status = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _cashSessionService.GetAllAsync(status, includeInactive, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtém os detalhes de uma sessão de caixa pelo seu identificador único.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CashSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CashSessionResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _cashSessionService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retorna a sessão de caixa atualmente aberta, se houver.
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(CashSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CashSessionResponse>> GetCurrent(
        CancellationToken cancellationToken = default)
    {
        var result = await _cashSessionService.GetCurrentOpenAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Abre uma nova sessão de caixa (turno) com fundo de troco inicial.
    /// Apenas uma sessão pode estar aberta por vez.
    /// </summary>
    [HttpPost("open")]
    [ProducesResponseType(typeof(CashSessionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CashSessionResponse>> Open(
        [FromBody] OpenCashSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _cashSessionService.OpenAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Registra um suprimento (aporte de dinheiro) na sessão de caixa.
    /// </summary>
    [HttpPost("{id:guid}/supply")]
    [ProducesResponseType(typeof(CashMovementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CashMovementResponse>> AddSupply(
        Guid id,
        [FromBody] AddCashMovementRequest request,
        CancellationToken cancellationToken = default)
    {
        var supplyRequest = request with { Type = CashMovementType.Supply };
        var result = await _cashSessionService.AddMovementAsync(id, supplyRequest, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registra uma sangria (retirada de dinheiro) na sessão de caixa.
    /// O valor não pode exceder o saldo de dinheiro disponível na gaveta.
    /// </summary>
    [HttpPost("{id:guid}/withdrawal")]
    [ProducesResponseType(typeof(CashMovementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CashMovementResponse>> AddWithdrawal(
        Guid id,
        [FromBody] AddCashMovementRequest request,
        CancellationToken cancellationToken = default)
    {
        var withdrawalRequest = request with { Type = CashMovementType.Withdrawal };
        var result = await _cashSessionService.AddMovementAsync(id, withdrawalRequest, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Fecha a sessão de caixa com conferência cega.
    /// O operador declara o valor contado na gaveta e o sistema calcula a divergência.
    /// </summary>
    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(typeof(CashSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CashSessionResponse>> Close(
        Guid id,
        [FromBody] CloseCashSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _cashSessionService.CloseAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
