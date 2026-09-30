using Komari.Application.DeliveryOrders.DTOs;
using Komari.Application.DeliveryOrders.Interfaces;
using Komari.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Komari.Api.Controllers;

public class DeliveryOrdersController : ApiControllerBase
{
    private readonly IDeliveryOrderService _deliveryOrderService;

    public DeliveryOrdersController(IDeliveryOrderService deliveryOrderService)
    {
        _deliveryOrderService = deliveryOrderService;
    }

    /// <summary>
    /// Lista pedidos de entrega em domicílio com filtros opcionais por status de produção/entrega e por cliente.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DeliveryOrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DeliveryOrderResponse>>> GetAll(
        [FromQuery] OrderStatus? status = null,
        [FromQuery] Guid? customerId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _deliveryOrderService.GetAllAsync(status, customerId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtém os detalhes completos de um pedido de delivery pelo seu identificador (ID).
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DeliveryOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeliveryOrderResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _deliveryOrderService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registra um novo pedido para entrega em domicílio, associando cliente, endereço, itens e cálculo de taxa.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DeliveryOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DeliveryOrderResponse>> Create(
        [FromBody] CreateDeliveryOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _deliveryOrderService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Despacha um pedido pronto da cozinha para rota de entrega em trânsito com motoboy/entregador.
    /// </summary>
    [HttpPatch("{id:guid}/dispatch")]
    [ProducesResponseType(typeof(DeliveryOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeliveryOrderResponse>> Dispatch(
        Guid id,
        [FromBody] DispatchDeliveryOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _deliveryOrderService.DispatchAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Confirma a conclusão da entrega do pedido de delivery no endereço do cliente final.
    /// </summary>
    [HttpPatch("{id:guid}/deliver")]
    [ProducesResponseType(typeof(DeliveryOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeliveryOrderResponse>> Deliver(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _deliveryOrderService.DeliverAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cancela um pedido de delivery informando o motivo ou justificativa.
    /// </summary>
    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(DeliveryOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeliveryOrderResponse>> Cancel(
        Guid id,
        [FromBody] CancelDeliveryOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _deliveryOrderService.CancelAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Atualiza o tempo estimado de entrega em minutos.
    /// </summary>
    [HttpPatch("{id:guid}/estimated-minutes")]
    [ProducesResponseType(typeof(DeliveryOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeliveryOrderResponse>> UpdateEstimatedMinutes(
        Guid id,
        [FromBody] UpdateEstimatedMinutesRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _deliveryOrderService.UpdateEstimatedMinutesAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
