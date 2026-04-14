using PawNect.Application.DTOs.Diagnostic;

namespace PawNect.Application.Interfaces;

public interface IDiagnosticReportService
{
    Task<DiagnosticReportDto?> GetByIdAsync(int reportId);
    Task<DiagnosticReportDto?> GetByDiagnosticOrderIdAsync(int diagnosticOrderId);
    Task<DiagnosticReportDto?> GetByConsultationIdAsync(int consultationId);
    Task<DiagnosticReportDto> SaveReportAsync(int diagnosticOrderId, string? reportFileUrl, string? reportFileName, SaveDiagnosticReportDto dto);
}
