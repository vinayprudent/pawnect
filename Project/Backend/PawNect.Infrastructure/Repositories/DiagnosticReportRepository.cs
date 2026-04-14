using Microsoft.EntityFrameworkCore;
using PawNect.Application.Interfaces;
using PawNect.Domain.Entities;
using PawNect.Infrastructure.DbContext;

namespace PawNect.Infrastructure.Repositories;

public class DiagnosticReportRepository : Repository<DiagnosticReport>, IDiagnosticReportRepository
{
    public DiagnosticReportRepository(PawNectDbContext context) : base(context)
    {
    }

    public async Task<DiagnosticReport?> GetByDiagnosticOrderIdAsync(int diagnosticOrderId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.DiagnosticOrderId == diagnosticOrderId && !r.IsDeleted);
    }

    public async Task<DiagnosticReport?> GetByConsultationIdAsync(int consultationId)
    {
        return await _dbSet
            .Include(r => r.DiagnosticOrder)
            .FirstOrDefaultAsync(r => !r.IsDeleted && r.DiagnosticOrder != null && r.DiagnosticOrder.ConsultationId == consultationId && !r.DiagnosticOrder.IsDeleted);
    }
}
