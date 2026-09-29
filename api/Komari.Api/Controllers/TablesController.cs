using Komari.Application.Tables.DTOs;
using Komari.Application.Tables.Interfaces;
using Komari.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Komari.Api.Controllers;

public class TablesController : ApiControllerBase
{
    private readonly ITableService _tableService;

    public TablesController(ITableService tableService)
    {
        _tableService = tableService;
    }

    /// <summary>
    /// Lista as mesas e posições de balcão cadastradas, com filtros opcionais por tipo e status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TableResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TableResponse>>> GetAll(
        [FromQuery] TableType? type = null,
        [FromQuery] TableStatus? status = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _tableService.GetAllAsync(type, status, includeInactive, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtém os detalhes de uma mesa ou posição de balcão pelo ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TableResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TableResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _tableService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cadastra uma nova mesa de salão ou posição de balcão.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TableResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TableResponse>> Create(
        [FromBody] CreateTableRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _tableService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Inicializa ou sincroniza em lote as N mesas de salão do restaurante (ex: 1 a 20).
    /// </summary>
    [HttpPost("initialize")]
    [ProducesResponseType(typeof(IReadOnlyList<TableResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<TableResponse>>> Initialize(
        [FromBody] InitializeTablesRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _tableService.InitializeTablesAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Atualiza os dados cadastrais de uma mesa ou posição de balcão.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TableResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TableResponse>> Update(
        Guid id,
        [FromBody] UpdateTableRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _tableService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Atualiza rapidamente o status operacional da mesa (Disponível, Ocupada, Reservada).
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateTableStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        await _tableService.UpdateStatusAsync(id, request.Status, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Inativa uma mesa ou posição de balcão (Soft Delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _tableService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
