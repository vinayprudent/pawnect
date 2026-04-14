using PawNect.Domain.Entities;

namespace PawNect.Application.Interfaces;

public interface IDiagnosticReportRepository : IRepository<DiagnosticReport>
{
    Task<DiagnosticReport?> GetByDiagnosticOrderIdAsync(int diagnosticOrderId);
    Task<DiagnosticReport?> GetByConsultationIdAsync(int consultationId);
}
