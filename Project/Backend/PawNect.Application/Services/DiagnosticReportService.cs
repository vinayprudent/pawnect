using PawNect.Application.DTOs.Diagnostic;
using PawNect.Application.Interfaces;
using PawNect.Domain.Entities;

namespace PawNect.Application.Services;

public class DiagnosticReportService : IDiagnosticReportService
{
    private readonly IDiagnosticReportRepository _reportRepository;
    private readonly IDiagnosticOrderRepository _orderRepository;

    public DiagnosticReportService(IDiagnosticReportRepository reportRepository, IDiagnosticOrderRepository orderRepository)
    {
        _reportRepository = reportRepository;
        _orderRepository = orderRepository;
    }

    public async Task<DiagnosticReportDto?> GetByIdAsync(int reportId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        return report == null ? null : MapToDto(report);
    }

    public async Task<DiagnosticReportDto?> GetByDiagnosticOrderIdAsync(int diagnosticOrderId)
    {
        var report = await _reportRepository.GetByDiagnosticOrderIdAsync(diagnosticOrderId);
        return report == null ? null : MapToDto(report);
    }

    public async Task<DiagnosticReportDto?> GetByConsultationIdAsync(int consultationId)
    {
        var report = await _reportRepository.GetByConsultationIdAsync(consultationId);
        return report == null ? null : MapToDto(report);
    }

    public async Task<DiagnosticReportDto> SaveReportAsync(int diagnosticOrderId, string? reportFileUrl, string? reportFileName, SaveDiagnosticReportDto dto)
    {
        var order = await _orderRepository.GetByIdWithLinesAsync(diagnosticOrderId);
        if (order == null)
            throw new ArgumentException("Diagnostic order not found");
        var existing = await _reportRepository.GetByDiagnosticOrderIdAsync(diagnosticOrderId);
        var now = DateTime.UtcNow;
        if (existing != null)
        {
            if (reportFileUrl != null) existing.ReportFileUrl = reportFileUrl;
            if (reportFileName != null) existing.ReportFileName = reportFileName;
            existing.VetAdvice = dto.VetAdvice;
            existing.NextSteps = dto.NextSteps;
            existing.ReviewedAt = now;
            existing.UpdatedAt = now;
            await _reportRepository.UpdateAsync(existing);
            await _reportRepository.SaveChangesAsync();
            return MapToDto(existing);
        }
        var report = new DiagnosticReport
        {
            DiagnosticOrderId = diagnosticOrderId,
            ReportFileUrl = reportFileUrl,
            ReportFileName = reportFileName,
            VetAdvice = dto.VetAdvice,
            NextSteps = dto.NextSteps,
            ReviewedAt = now
        };
        var added = await _reportRepository.AddAsync(report);
        await _reportRepository.SaveChangesAsync();
        return MapToDto(added);
    }

    private static DiagnosticReportDto MapToDto(DiagnosticReport r)
    {
        return new DiagnosticReportDto
        {
            Id = r.Id,
            DiagnosticOrderId = r.DiagnosticOrderId,
            ReportFileUrl = r.ReportFileUrl,
            ReportFileName = r.ReportFileName,
            VetAdvice = r.VetAdvice,
            NextSteps = r.NextSteps,
            ReviewedAt = r.ReviewedAt
        };
    }
}
