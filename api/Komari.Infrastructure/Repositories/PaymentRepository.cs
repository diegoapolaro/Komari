using Komari.Domain.Entities;
using Komari.Domain.Repositories;
using Komari.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Komari.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly KomariDbContext _context;

    public PaymentRepository(KomariDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Bill)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByBillIdAsync(Guid billId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Bill)
            .AsNoTracking()
            .Where(p => p.BillId == billId && p.IsActive)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByCashSessionIdAsync(Guid cashSessionId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Bill)
            .AsNoTracking()
            .Where(p => p.CashSessionId == cashSessionId && p.IsActive)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _context.Payments.AddAsync(payment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
