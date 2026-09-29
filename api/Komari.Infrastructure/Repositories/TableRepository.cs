using Komari.Domain.Entities;
using Komari.Domain.Enums;
using Komari.Domain.Repositories;
using Komari.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Komari.Infrastructure.Repositories;

public class TableRepository : ITableRepository
{
    private readonly KomariDbContext _context;

    public TableRepository(KomariDbContext context)
    {
        _context = context;
    }

    public async Task<Table?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tables
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Table>> GetAllAsync(
        TableType? type = null,
        TableStatus? status = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Table> query = _context.Tables.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        if (type.HasValue)
        {
            query = query.Where(t => t.Type == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        return await query
            .OrderBy(t => t.Type)
            .ThenBy(t => t.Number)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNumberAndTypeAsync(int number, TableType type, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Tables.Where(t => t.Number == number && t.Type == type && t.IsActive);

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Table table, CancellationToken cancellationToken = default)
    {
        await _context.Tables.AddAsync(table, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Table table, CancellationToken cancellationToken = default)
    {
        _context.Tables.Update(table);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
