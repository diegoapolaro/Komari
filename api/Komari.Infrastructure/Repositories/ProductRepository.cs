using Komari.Domain.Entities;
using Komari.Domain.Repositories;
using Komari.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Komari.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly KomariDbContext _context;

    public ProductRepository(KomariDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(Guid? categoryId = null, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category);

        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        return await query
            .OrderBy(p => p.Category.DisplayOrder)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLower();
        var query = _context.Products.Where(p => p.Name.ToLower() == normalized && p.IsActive);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
