using Komari.Application.Customers.DTOs;

namespace Komari.Application.Customers.Interfaces;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerResponse>> GetAllAsync(string? searchTerm = null, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerResponse> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);
    Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<CustomerResponse> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
