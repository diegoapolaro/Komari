using Komari.Application.Orders.DTOs;
using Komari.Application.Orders.Interfaces;
using Komari.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Komari.Api.Controllers;

public class OrdersController : ApiControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Lista pedidos com filtros opcionais por comanda e status de produção.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetAll(
        [FromQuery] Guid? billId = null,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetAllAsync(billId, status, includeInactive, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtém os detalhes de um pedido pelo seu ID, incluindo todos os itens de consumo.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lista todos os pedidos lançados para uma comanda específica.
    /// </summary>
    [HttpGet("bill/{billId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetByBillId(
        Guid billId,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetByBillIdAsync(billId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cria e envia um novo pedido de itens do cardápio para uma comanda aberta (mesa ou balcão).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderResponse>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Adiciona novos itens a um pedido existente que ainda esteja no status 'Pending'.
    /// </summary>
    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderResponse>> AddItems(
        Guid id,
        [FromBody] AddItemsToOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.AddItemsAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Atualiza o status do pedido no fluxo de produção da cozinha (KDS) ou entrega.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponse>> UpdateStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.UpdateStatusAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cancela um pedido com justificativa obrigatória caso já esteja em produção ou pronto.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponse>> Cancel(
        Guid id,
        [FromBody] CancelOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.CancelAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
