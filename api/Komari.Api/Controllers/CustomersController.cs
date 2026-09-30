using Komari.Application.Customers.DTOs;
using Komari.Application.Customers.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Komari.Api.Controllers;

public class CustomersController : ApiControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Lista os clientes cadastrados com suporte a busca por nome ou telefone e inclusão de inativos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CustomerResponse>>> GetAll(
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _customerService.GetAllAsync(searchTerm, includeInactive, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtém os dados de um cliente pelo identificador único.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _customerService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Localiza um cliente pelo número de telefone cadastrado.
    /// </summary>
    [HttpGet("by-phone/{phone}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerResponse>> GetByPhone(
        string phone,
        CancellationToken cancellationToken = default)
    {
        var result = await _customerService.GetByPhoneAsync(phone, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cadastra um novo cliente no sistema. O nome é obrigatório.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerResponse>> Create(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _customerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Atualiza os dados cadastrais de um cliente existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerResponse>> Update(
        Guid id,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _customerService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Desativa um cliente do sistema (Soft Delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _customerService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Lista todos os endereços ativos de entrega cadastrados para o cliente.
    /// </summary>
    [HttpGet("{id:guid}/addresses")]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerAddressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CustomerAddressResponse>>> GetAddresses(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _customerService.GetAddressesAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cadastra um novo endereço de entrega para o cliente.
    /// </summary>
    [HttpPost("{id:guid}/addresses")]
    [ProducesResponseType(typeof(CustomerAddressResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerAddressResponse>> AddAddress(
        Guid id,
        [FromBody] CreateCustomerAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _customerService.AddAddressAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetAddresses), new { id }, result);
    }

    /// <summary>
    /// Atualiza os dados de um endereço de entrega existente do cliente.
    /// </summary>
    [HttpPut("{id:guid}/addresses/{addressId:guid}")]
    [ProducesResponseType(typeof(CustomerAddressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerAddressResponse>> UpdateAddress(
        Guid id,
        Guid addressId,
        [FromBody] UpdateCustomerAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _customerService.UpdateAddressAsync(id, addressId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Define um endereço específico como o endereço padrão de entrega do cliente.
    /// </summary>
    [HttpPatch("{id:guid}/addresses/{addressId:guid}/default")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefaultAddress(
        Guid id,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        await _customerService.SetDefaultAddressAsync(id, addressId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Remove (inativa) um endereço de entrega do cliente.
    /// </summary>
    [HttpDelete("{id:guid}/addresses/{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAddress(
        Guid id,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        await _customerService.DeleteAddressAsync(id, addressId, cancellationToken);
        return NoContent();
    }
}
