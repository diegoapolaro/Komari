using Komari.Domain.Entities;
using Komari.Domain.Enums;
using Komari.Domain.Repositories;
using Komari.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Komari.Infrastructure.Repositories;

public class CashSessionRepository : ICashSessionRepository
{
    private readonly KomariDbContext _context;

    public CashSessionRepository(KomariDbContext context)
    {
        _context = context;
    }

    public async Task<CashSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CashSessions
            .Include(cs => cs.Payments)
            .Include(cs => cs.Movements)
            .FirstOrDefaultAsync(cs => cs.Id == id, cancellationToken);
    }

    public async Task<CashSession?> GetCurrentOpenAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CashSessions
            .Include(cs => cs.Payments)
            .Include(cs => cs.Movements)
            .FirstOrDefaultAsync(cs => cs.Status == CashSessionStatus.Open && cs.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<CashSession>> GetAllAsync(
        CashSessionStatus? status = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.CashSessions
            .Include(cs => cs.Payments)
            .Include(cs => cs.Movements)
            .AsNoTracking()
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(cs => cs.IsActive);

        if (status.HasValue)
            query = query.Where(cs => cs.Status == status.Value);

        return await query
            .OrderByDescending(cs => cs.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CashSession session, CancellationToken cancellationToken = default)
    {
        await _context.CashSessions.AddAsync(session, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CashSession session, CancellationToken cancellationToken = default)
    {
        var sessionEntry = _context.Entry(session);
        if (sessionEntry.State == EntityState.Detached)
        {
            _context.CashSessions.Update(session);
        }

        foreach (var movement in session.Movements)
        {
            var movementEntry = _context.Entry(movement);
            if (movementEntry.State == EntityState.Detached)
            {
                await _context.CashMovements.AddAsync(movement, cancellationToken);
            }
            else if (movementEntry.State == EntityState.Modified)
            {
                bool exists = await _context.CashMovements.AsNoTracking().AnyAsync(m => m.Id == movement.Id, cancellationToken);
                if (!exists)
                {
                    movementEntry.State = EntityState.Added;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
