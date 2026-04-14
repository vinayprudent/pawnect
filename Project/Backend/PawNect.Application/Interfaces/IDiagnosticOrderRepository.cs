using PawNect.Domain.Entities;

namespace PawNect.Application.Interfaces;

public interface IDiagnosticOrderRepository : IRepository<DiagnosticOrder>
{
    Task<DiagnosticOrder?> GetByConsultationIdWithLinesAsync(int consultationId);
    Task<DiagnosticOrder?> GetByIdWithLinesAsync(int orderId);
    Task<DiagnosticOrder> AddWithLinesAsync(DiagnosticOrder order, IEnumerable<DiagnosticOrderLine> lines);
}
