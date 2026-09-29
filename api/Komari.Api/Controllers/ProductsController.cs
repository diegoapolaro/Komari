using Komari.Application.Products.DTOs;
using Komari.Application.Products.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Komari.Api.Controllers;

public class ProductsController : ApiControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Lista os produtos do cardápio, permitindo filtrar por categoria.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetAllAsync(categoryId, includeInactive, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtém os detalhes de um produto por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cadastra um novo produto no cardápio.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Atualiza os dados de um produto existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Altera a disponibilidade de um produto (ex: pausar item quando acaba ingrediente).
    /// </summary>
    [HttpPatch("{id:guid}/availability")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetAvailability(
        Guid id,
        [FromBody] UpdateProductAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        await _productService.SetAvailabilityAsync(id, request.IsAvailable, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Desativa um produto do cardápio (Soft Delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _productService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
