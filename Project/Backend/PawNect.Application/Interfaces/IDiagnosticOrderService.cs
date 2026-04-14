using PawNect.Application.DTOs.Diagnostic;

namespace PawNect.Application.Interfaces;

public interface IDiagnosticOrderService
{
    Task<DiagnosticOrderDto?> GetByConsultationIdAsync(int consultationId);
    Task<DiagnosticOrderDto?> GetByIdAsync(int orderId);
    Task<DiagnosticOrderDto> CreateOrderAsync(CreateDiagnosticOrderDto dto);
    Task<DiagnosticOrderDto?> UpdateStatusAsync(int orderId, UpdateDiagnosticOrderStatusDto dto);
}
