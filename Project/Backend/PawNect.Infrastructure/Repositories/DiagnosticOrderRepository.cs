using Microsoft.EntityFrameworkCore;
using PawNect.Application.Interfaces;
using PawNect.Domain.Entities;
using PawNect.Infrastructure.DbContext;

namespace PawNect.Infrastructure.Repositories;

public class DiagnosticOrderRepository : Repository<DiagnosticOrder>, IDiagnosticOrderRepository
{
    public DiagnosticOrderRepository(PawNectDbContext context) : base(context)
    {
    }

    public async Task<DiagnosticOrder?> GetByConsultationIdWithLinesAsync(int consultationId)
    {
        return await _dbSet
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.ConsultationId == consultationId && !d.IsDeleted);
    }

    public async Task<DiagnosticOrder?> GetByIdWithLinesAsync(int orderId)
    {
        return await _dbSet
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == orderId && !d.IsDeleted);
    }

    public async Task<DiagnosticOrder> AddWithLinesAsync(DiagnosticOrder order, IEnumerable<DiagnosticOrderLine> lines)
    {
        await _dbSet.AddAsync(order);
        await _context.SaveChangesAsync();
        foreach (var line in lines)
        {
            line.DiagnosticOrderId = order.Id;
            _context.Set<DiagnosticOrderLine>().Add(line);
        }
        await _context.SaveChangesAsync();
        return await GetByConsultationIdWithLinesAsync(order.ConsultationId) ?? order;
    }
}
