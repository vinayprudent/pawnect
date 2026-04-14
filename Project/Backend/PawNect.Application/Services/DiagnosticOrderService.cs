using PawNect.Application.DTOs.Diagnostic;
using PawNect.Application.Interfaces;
using PawNect.Domain.Entities;

namespace PawNect.Application.Services;

public class DiagnosticOrderService : IDiagnosticOrderService
{
    private readonly IDiagnosticOrderRepository _orderRepository;
    private readonly IConsultationRepository _consultationRepository;

    public DiagnosticOrderService(IDiagnosticOrderRepository orderRepository, IConsultationRepository consultationRepository)
    {
        _orderRepository = orderRepository;
        _consultationRepository = consultationRepository;
    }

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Ordered", "Sample Collected", "Processing", "Report Uploaded", "Report Available", "Reviewed"
    };

    public async Task<DiagnosticOrderDto?> GetByConsultationIdAsync(int consultationId)
    {
        var order = await _orderRepository.GetByConsultationIdWithLinesAsync(consultationId);
        return order == null ? null : MapToDto(order);
    }

    public async Task<DiagnosticOrderDto?> GetByIdAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdWithLinesAsync(orderId);
        return order == null ? null : MapToDto(order);
    }

    public async Task<DiagnosticOrderDto> CreateOrderAsync(CreateDiagnosticOrderDto dto)
    {
        var consultation = await _consultationRepository.GetByIdAsync(dto.ConsultationId);
        if (consultation == null)
            throw new ArgumentException("Consultation not found");

        var total = dto.Tests.Sum(t => t.Price);
        var order = new DiagnosticOrder
        {
            ConsultationId = dto.ConsultationId,
            PetId = dto.PetId,
            OwnerId = dto.OwnerId,
            VetId = dto.VetId,
            Status = "Ordered",
            CollectionType = dto.CollectionType,
            AssignedLabName = dto.AssignedLabName ?? "PawNect Lab – Auto-assigned",
            TotalPrice = total,
            OrderedAt = DateTime.UtcNow
        };
        var lines = dto.Tests.Select(t => new DiagnosticOrderLine
        {
            TestName = t.TestName,
            LabTestCatalogItemId = t.LabTestCatalogItemId,
            Price = t.Price
        }).ToList();
        var saved = await _orderRepository.AddWithLinesAsync(order, lines);
        return MapToDto(saved);
    }

    public async Task<DiagnosticOrderDto?> UpdateStatusAsync(int orderId, UpdateDiagnosticOrderStatusDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Status) || !AllowedStatuses.Contains(dto.Status.Trim()))
            return null;
        var order = await _orderRepository.GetByIdWithLinesAsync(orderId);
        if (order == null) return null;
        var status = dto.Status.Trim();
        order.Status = status;
        var now = DateTime.UtcNow;
        if (status.Equals("Sample Collected", StringComparison.OrdinalIgnoreCase) && order.SampleCollectedAt == null)
            order.SampleCollectedAt = now;
        if ((status.Equals("Report Uploaded", StringComparison.OrdinalIgnoreCase) || status.Equals("Report Available", StringComparison.OrdinalIgnoreCase)) && order.ReportUploadedAt == null)
            order.ReportUploadedAt = now;
        order.UpdatedAt = now;
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();
        var updated = await _orderRepository.GetByIdWithLinesAsync(orderId);
        return updated == null ? null : MapToDto(updated);
    }

    private static DiagnosticOrderDto MapToDto(DiagnosticOrder o)
    {
        return new DiagnosticOrderDto
        {
            Id = o.Id,
            ConsultationId = o.ConsultationId,
            PetId = o.PetId,
            OwnerId = o.OwnerId,
            VetId = o.VetId,
            Status = o.Status,
            CollectionType = o.CollectionType,
            AssignedLabName = o.AssignedLabName,
            TotalPrice = o.TotalPrice,
            OrderedAt = o.OrderedAt,
            SampleCollectedAt = o.SampleCollectedAt,
            ReportUploadedAt = o.ReportUploadedAt,
            TestsOrdered = o.Lines?.Select(l => l.TestName).ToList() ?? new List<string>()
        };
    }
}
