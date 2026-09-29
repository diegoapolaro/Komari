using Komari.Application.Bills.DTOs;
using Komari.Application.Bills.Interfaces;
using Komari.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Komari.Api.Controllers;

public class BillsController : ApiControllerBase
{
    private readonly IBillService _billService;

    public BillsController(IBillService billService)
    {
        _billService = billService;
    }

    /// <summary>
    /// Lista as comandas cadastradas com filtros opcionais por status e mesa.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BillResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BillResponse>>> GetAll(
        [FromQuery] BillStatus? status = null,
        [FromQuery] Guid? tableId = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _billService.GetAllAsync(status, tableId, includeInactive, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtém os detalhes de uma comanda pelo seu identificador único (ID).
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BillResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BillResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _billService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Localiza a comanda que está atualmente aberta pelo seu número físico sequencial.
    /// </summary>
    [HttpGet("number/{number:int}")]
    [ProducesResponseType(typeof(BillResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BillResponse>> GetActiveByNumber(
        int number,
        CancellationToken cancellationToken = default)
    {
        var result = await _billService.GetActiveByNumberAsync(number, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Abre uma nova comanda de consumo (vinculada a uma mesa ou avulsa/balcão).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BillResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BillResponse>> Open(
        [FromBody] OpenBillRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _billService.OpenAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Solicita o fechamento da conta da comanda (coloca em conferência 'Closing' / status laranja e bloqueia novos itens).
    /// </summary>
    [HttpPost("{id:guid}/request-closing")]
    [ProducesResponseType(typeof(BillResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BillResponse>> RequestClosing(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _billService.RequestClosingAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Reabre uma comanda que estava em conferência/fechamento voltando para status 'Open'.
    /// </summary>
    [HttpPost("{id:guid}/reopen")]
    [ProducesResponseType(typeof(BillResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BillResponse>> Reopen(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _billService.ReopenAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Atualiza observações e/ou o nome do cliente associado à comanda/mesa/balcão.
    /// </summary>
    [HttpPatch("{id:guid}/details")]
    [ProducesResponseType(typeof(BillResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BillResponse>> UpdateDetails(
        Guid id,
        [FromBody] UpdateBillDetailsRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _billService.UpdateDetailsAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Encerra uma comanda de consumo, liberando a mesa caso não restem outras comandas ativas nela.
    /// </summary>
    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(typeof(BillResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BillResponse>> Close(
        Guid id,
        [FromBody] CloseBillRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _billService.CloseAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cancela uma comanda de consumo sem cobrança, registrando a justificativa para fins de auditoria.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(BillResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BillResponse>> Cancel(
        Guid id,
        [FromBody] CancelBillRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _billService.CancelAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
