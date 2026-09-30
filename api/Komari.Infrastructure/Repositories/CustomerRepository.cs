using Komari.Domain.Entities;
using Komari.Domain.Repositories;
using Komari.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Komari.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly KomariDbContext _context;

    public CustomerRepository(KomariDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByIdWithAddressesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(
        string? searchTerm = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Customer> query = _context.Customers.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            var digitsOnly = new string(term.Where(char.IsDigit).ToArray());

            if (!string.IsNullOrEmpty(digitsOnly) && digitsOnly.Length >= 2)
            {
                query = query.Where(c =>
                    c.Name.ToLower().Contains(term) ||
                    (c.Phone != null && (c.Phone.ToLower().Contains(term) || c.Phone.Contains(digitsOnly))));
            }
            else
            {
                query = query.Where(c =>
                    c.Name.ToLower().Contains(term) ||
                    (c.Phone != null && c.Phone.ToLower().Contains(term)));
            }
        }

        return await query
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        var normalizedPhone = phone.Trim();
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Phone == normalizedPhone && c.IsActive, cancellationToken);
    }

    public async Task<bool> ExistsByPhoneAsync(string phone, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedPhone = phone.Trim();
        var query = _context.Customers.Where(c => c.Phone == normalizedPhone && c.IsActive);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(customer);
        if (entry.State == EntityState.Detached)
        {
            _context.Customers.Update(customer);
        }

        foreach (var address in customer.Addresses)
        {
            var addressEntry = _context.Entry(address);
            if (addressEntry.State == EntityState.Detached)
            {
                await _context.CustomerAddresses.AddAsync(address, cancellationToken);
            }
            else if (addressEntry.State == EntityState.Modified)
            {
                bool exists = await _context.CustomerAddresses.AsNoTracking().AnyAsync(a => a.Id == address.Id, cancellationToken);
                if (!exists)
                {
                    addressEntry.State = EntityState.Added;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
