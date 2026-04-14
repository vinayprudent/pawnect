using Microsoft.AspNetCore.Mvc;
using PawNect.Application.DTOs;
using PawNect.Application.DTOs.Diagnostic;
using PawNect.Application.Interfaces;
using PawNect.Domain.Entities;

namespace PawNect.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly IDiagnosticOrderService _orderService;
    private readonly IDiagnosticReportService _reportService;
    private readonly IRepository<LabTestCatalogItem> _catalogRepository;
    private readonly ILogger<DiagnosticsController> _logger;
    private readonly IWebHostEnvironment _env;

    public DiagnosticsController(
        IDiagnosticOrderService orderService,
        IDiagnosticReportService reportService,
        IRepository<LabTestCatalogItem> catalogRepository,
        ILogger<DiagnosticsController> logger,
        IWebHostEnvironment env)
    {
        _orderService = orderService;
        _reportService = reportService;
        _catalogRepository = catalogRepository;
        _logger = logger;
        _env = env;
    }

    [HttpGet("catalog")]
    public async Task<ActionResult<ApiResponse<IEnumerable<LabTestCatalogItemDto>>>> GetCatalog()
    {
        try
        {
            var items = await _catalogRepository.GetAllAsync();
            var list = items.Where(i => !i.IsDeleted).Select(i => new LabTestCatalogItemDto
            {
                Id = i.Id,
                Name = i.Name,
                TestType = i.TestType,
                Price = i.Price,
                SampleType = i.SampleType,
                Description = i.Description,
                TestsIncludedJson = i.TestsIncludedJson
            });
            return Ok(ApiResponse<IEnumerable<LabTestCatalogItemDto>>.SuccessResponse(list, "Catalog retrieved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lab catalog");
            return StatusCode(500, ApiResponse<IEnumerable<LabTestCatalogItemDto>>.ErrorResponse("An error occurred"));
        }
    }

    [HttpPost("orders")]
    public async Task<ActionResult<ApiResponse<DiagnosticOrderDto>>> CreateOrder([FromBody] CreateDiagnosticOrderDto dto)
    {
        try
        {
            if (dto == null || dto.Tests == null || !dto.Tests.Any())
                return BadRequest(ApiResponse<DiagnosticOrderDto>.ErrorResponse("At least one test is required"));
            var order = await _orderService.CreateOrderAsync(dto);
            return Ok(ApiResponse<DiagnosticOrderDto>.SuccessResponse(order, "Diagnostic order created successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<DiagnosticOrderDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating diagnostic order");
            return StatusCode(500, ApiResponse<DiagnosticOrderDto>.ErrorResponse("An error occurred while creating the order"));
        }
    }

    [HttpGet("orders/consultation/{consultationId}")]
    public async Task<ActionResult<ApiResponse<DiagnosticOrderDto>>> GetOrderByConsultation(int consultationId)
    {
        try
        {
            var order = await _orderService.GetByConsultationIdAsync(consultationId);
            if (order == null)
                return NotFound(ApiResponse<DiagnosticOrderDto>.ErrorResponse("No diagnostic order found for this consultation"));
            return Ok(ApiResponse<DiagnosticOrderDto>.SuccessResponse(order, "Order retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving diagnostic order");
            return StatusCode(500, ApiResponse<DiagnosticOrderDto>.ErrorResponse("An error occurred"));
        }
    }

    [HttpGet("orders/{orderId}")]
    public async Task<ActionResult<ApiResponse<DiagnosticOrderDto>>> GetOrderById(int orderId)
    {
        try
        {
            var order = await _orderService.GetByIdAsync(orderId);
            if (order == null)
                return NotFound(ApiResponse<DiagnosticOrderDto>.ErrorResponse("Diagnostic order not found"));
            return Ok(ApiResponse<DiagnosticOrderDto>.SuccessResponse(order, "Order retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving diagnostic order {OrderId}", orderId);
            return StatusCode(500, ApiResponse<DiagnosticOrderDto>.ErrorResponse("An error occurred"));
        }
    }

    [HttpPatch("orders/{orderId}/status")]
    public async Task<ActionResult<ApiResponse<DiagnosticOrderDto>>> UpdateOrderStatus(int orderId, [FromBody] UpdateDiagnosticOrderStatusDto dto)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Status))
                return BadRequest(ApiResponse<DiagnosticOrderDto>.ErrorResponse("Status is required"));
            var order = await _orderService.UpdateStatusAsync(orderId, dto);
            if (order == null)
                return NotFound(ApiResponse<DiagnosticOrderDto>.ErrorResponse("Diagnostic order not found or invalid status"));
            return Ok(ApiResponse<DiagnosticOrderDto>.SuccessResponse(order, "Status updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating diagnostic order {OrderId} status", orderId);
            return StatusCode(500, ApiResponse<DiagnosticOrderDto>.ErrorResponse("An error occurred"));
        }
    }

    [HttpGet("reports/consultation/{consultationId}")]
    public async Task<ActionResult<ApiResponse<DiagnosticReportDto>>> GetReportByConsultation(int consultationId)
    {
        try
        {
            var report = await _reportService.GetByConsultationIdAsync(consultationId);
            if (report == null)
                return NotFound(ApiResponse<DiagnosticReportDto>.ErrorResponse("No report found for this consultation"));
            return Ok(ApiResponse<DiagnosticReportDto>.SuccessResponse(report, "Report retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving report for consultation {ConsultationId}", consultationId);
            return StatusCode(500, ApiResponse<DiagnosticReportDto>.ErrorResponse("An error occurred"));
        }
    }

    [HttpGet("reports/order/{diagnosticOrderId}")]
    public async Task<ActionResult<ApiResponse<DiagnosticReportDto>>> GetReportByOrder(int diagnosticOrderId)
    {
        try
        {
            var report = await _reportService.GetByDiagnosticOrderIdAsync(diagnosticOrderId);
            if (report == null)
                return NotFound(ApiResponse<DiagnosticReportDto>.ErrorResponse("No report found for this order"));
            return Ok(ApiResponse<DiagnosticReportDto>.SuccessResponse(report, "Report retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving report for order {OrderId}", diagnosticOrderId);
            return StatusCode(500, ApiResponse<DiagnosticReportDto>.ErrorResponse("An error occurred"));
        }
    }

    [HttpPost("reports")]
    [RequestSizeLimit(10_485_760)] // 10 MB
    public async Task<ActionResult<ApiResponse<DiagnosticReportDto>>> UploadReport([FromForm] int diagnosticOrderId, [FromForm] string? vetAdvice, [FromForm] string? nextSteps, IFormFile? file)
    {
        try
        {
            string? reportFileUrl = null;
            string? reportFileName = null;
            if (file != null && file.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.ContentRootPath, "uploads", "reports");
                Directory.CreateDirectory(uploadsDir);
                var ext = Path.GetExtension(file.FileName) ?? ".pdf";
                var fileName = $"report_{diagnosticOrderId}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);
                await using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);
                reportFileUrl = $"uploads/reports/{fileName}";
                reportFileName = file.FileName;
            }
            var dto = new SaveDiagnosticReportDto { DiagnosticOrderId = diagnosticOrderId, VetAdvice = vetAdvice, NextSteps = nextSteps };
            var report = await _reportService.SaveReportAsync(diagnosticOrderId, reportFileUrl, reportFileName, dto);
            return Ok(ApiResponse<DiagnosticReportDto>.SuccessResponse(report, "Report saved successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<DiagnosticReportDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving diagnostic report");
            return StatusCode(500, ApiResponse<DiagnosticReportDto>.ErrorResponse("An error occurred"));
        }
    }

    [HttpGet("report-file/{reportId}")]
    public async Task<IActionResult> GetReportFile(int reportId)
    {
        var report = await _reportService.GetByIdAsync(reportId);
        if (report == null || string.IsNullOrEmpty(report.ReportFileUrl))
            return NotFound();
        var physicalPath = Path.Combine(_env.ContentRootPath, report.ReportFileUrl);
        if (!System.IO.File.Exists(physicalPath))
            return NotFound();
        var contentType = "application/pdf";
        if (physicalPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) contentType = "image/png";
        else if (physicalPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || physicalPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)) contentType = "image/jpeg";
        return PhysicalFile(physicalPath, contentType, report.ReportFileName ?? Path.GetFileName(physicalPath));
    }
}
