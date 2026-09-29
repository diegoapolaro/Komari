using Komari.Domain.Entities;
using Komari.Domain.Enums;
using Komari.Domain.Repositories;
using Komari.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Komari.Infrastructure.Repositories;

public class BillRepository : IBillRepository
{
    private readonly KomariDbContext _context;

    public BillRepository(KomariDbContext context)
    {
        _context = context;
    }

    public async Task<Bill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Bills
            .Include(b => b.Table)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Bill?> GetActiveByNumberAsync(int number, CancellationToken cancellationToken = default)
    {
        return await _context.Bills
            .Include(b => b.Table)
            .FirstOrDefaultAsync(b => b.Number == number && b.Status == BillStatus.Open && b.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetAllAsync(
        BillStatus? status = null,
        Guid? tableId = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Bill> query = _context.Bills
            .Include(b => b.Table)
            .AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(b => b.IsActive);
        }

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        if (tableId.HasValue)
        {
            query = query.Where(b => b.TableId == tableId.Value);
        }

        return await query
            .OrderByDescending(b => b.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsActiveByNumberAsync(int number, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Bills
            .Where(b => b.Number == number && b.Status == BillStatus.Open && b.IsActive);

        if (excludeId.HasValue)
        {
            query = query.Where(b => b.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int> CountActiveByTableIdAsync(Guid tableId, Guid? excludeBillId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Bills
            .Where(b => b.TableId == tableId && (b.Status == BillStatus.Open || b.Status == BillStatus.Closing) && b.IsActive);

        if (excludeBillId.HasValue)
        {
            query = query.Where(b => b.Id != excludeBillId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> GetNextNumberAsync(CancellationToken cancellationToken = default)
    {
        var maxNumber = await _context.Bills
            .Select(b => (int?)b.Number)
            .MaxAsync(cancellationToken);

        return (maxNumber ?? 0) + 1;
    }

    public async Task AddAsync(Bill bill, CancellationToken cancellationToken = default)
    {
        await _context.Bills.AddAsync(bill, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Bill bill, CancellationToken cancellationToken = default)
    {
        _context.Bills.Update(bill);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
