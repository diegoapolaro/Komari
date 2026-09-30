using FluentValidation;
using Komari.Application.Common.Exceptions;
using Komari.Application.Customers.DTOs;
using Komari.Application.Customers.Interfaces;
using Komari.Domain.Entities;
using Komari.Domain.Repositories;

namespace Komari.Application.Customers.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IValidator<CreateCustomerRequest> _createValidator;
    private readonly IValidator<UpdateCustomerRequest> _updateValidator;
    private readonly IValidator<CreateCustomerAddressRequest> _createAddressValidator;
    private readonly IValidator<UpdateCustomerAddressRequest> _updateAddressValidator;

    public CustomerService(
        ICustomerRepository customerRepository,
        IValidator<CreateCustomerRequest> createValidator,
        IValidator<UpdateCustomerRequest> updateValidator,
        IValidator<CreateCustomerAddressRequest> createAddressValidator,
        IValidator<UpdateCustomerAddressRequest> updateAddressValidator)
    {
        _customerRepository = customerRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _createAddressValidator = createAddressValidator;
        _updateAddressValidator = updateAddressValidator;
    }

    public async Task<IReadOnlyList<CustomerResponse>> GetAllAsync(
        string? searchTerm = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllAsync(searchTerm, includeInactive, cancellationToken);
        return customers.Select(MapToResponse).ToList();
    }

    public async Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), id);

        return MapToResponse(customer);
    }

    public async Task<CustomerResponse> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new ArgumentException("O telefone para busca deve ser informado.", nameof(phone));
        }

        var customer = await _customerRepository.GetByPhoneAsync(phone.Trim(), cancellationToken)
            ?? throw new NotFoundException($"Cliente com o telefone '{phone}' não foi encontrado.");

        return MapToResponse(customer);
    }

    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Phone) &&
            await _customerRepository.ExistsByPhoneAsync(request.Phone.Trim(), null, cancellationToken))
        {
            throw new ConflictException($"Já existe um cliente ativo cadastrado com o telefone '{request.Phone}'.");
        }

        var customer = new Customer(
            request.Name,
            request.Phone,
            request.Email,
            request.Document,
            request.Notes
        );

        await _customerRepository.AddAsync(customer, cancellationToken);

        return MapToResponse(customer);
    }

    public async Task<CustomerResponse> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), id);

        if (!string.IsNullOrWhiteSpace(request.Phone) &&
            await _customerRepository.ExistsByPhoneAsync(request.Phone.Trim(), id, cancellationToken))
        {
            throw new ConflictException($"Já existe outro cliente ativo cadastrado com o telefone '{request.Phone}'.");
        }

        customer.UpdateDetails(
            request.Name,
            request.Phone,
            request.Email,
            request.Document,
            request.Notes
        );

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        return MapToResponse(customer);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), id);

        customer.Deactivate();
        await _customerRepository.UpdateAsync(customer, cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerAddressResponse>> GetAddressesAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdWithAddressesAsync(customerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), customerId);

        return customer.Addresses
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .Select(MapToAddressResponse)
            .ToList();
    }

    public async Task<CustomerAddressResponse> AddAddressAsync(
        Guid customerId,
        CreateCustomerAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createAddressValidator.ValidateAndThrowAsync(request, cancellationToken);

        var customer = await _customerRepository.GetByIdWithAddressesAsync(customerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), customerId);

        var address = customer.AddAddress(
            request.Street,
            request.Number,
            request.Neighborhood,
            request.ZipCode,
            request.Complement,
            request.ReferencePoint,
            request.IsDefault
        );

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        return MapToAddressResponse(address);
    }

    public async Task<CustomerAddressResponse> UpdateAddressAsync(
        Guid customerId,
        Guid addressId,
        UpdateCustomerAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateAddressValidator.ValidateAndThrowAsync(request, cancellationToken);

        var customer = await _customerRepository.GetByIdWithAddressesAsync(customerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), customerId);

        customer.UpdateAddress(
            addressId,
            request.Street,
            request.Number,
            request.Neighborhood,
            request.ZipCode,
            request.Complement,
            request.ReferencePoint,
            request.IsDefault
        );

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        var updatedAddress = customer.Addresses.First(a => a.Id == addressId);
        return MapToAddressResponse(updatedAddress);
    }

    public async Task SetDefaultAddressAsync(
        Guid customerId,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdWithAddressesAsync(customerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), customerId);

        customer.SetDefaultAddress(addressId);

        await _customerRepository.UpdateAsync(customer, cancellationToken);
    }

    public async Task DeleteAddressAsync(
        Guid customerId,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdWithAddressesAsync(customerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), customerId);

        customer.RemoveAddress(addressId);

        await _customerRepository.UpdateAsync(customer, cancellationToken);
    }

    private static CustomerResponse MapToResponse(Customer customer) =>
        new(
            customer.Id,
            customer.Name,
            customer.Phone,
            customer.Email,
            customer.Document,
            customer.Notes,
            customer.IsActive,
            customer.CreatedAt,
            customer.UpdatedAt
        );

    private static CustomerAddressResponse MapToAddressResponse(CustomerAddress address) =>
        new(
            address.Id,
            address.CustomerId,
            address.Street,
            address.Number,
            address.Neighborhood,
            address.ZipCode,
            address.Complement,
            address.ReferencePoint,
            address.IsDefault,
            address.IsActive,
            address.CreatedAt,
            address.UpdatedAt
        );
}
