using Microsoft.AspNetCore.Mvc;
using PawNect.Application.DTOs;
using PawNect.Application.DTOs.Consultation;
using PawNect.Application.Interfaces;

namespace PawNect.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ConsultationsController : ControllerBase
{
    private readonly IConsultationService _consultationService;
    private readonly IConsultationRepository _consultationRepository;
    private readonly ILogger<ConsultationsController> _logger;
    private readonly IWebHostEnvironment _env;

    public ConsultationsController(IConsultationService consultationService, IConsultationRepository consultationRepository, ILogger<ConsultationsController> logger, IWebHostEnvironment env)
    {
        _consultationService = consultationService;
        _consultationRepository = consultationRepository;
        _logger = logger;
        _env = env;
    }

    [HttpGet("appointment/{appointmentId}")]
    public async Task<ActionResult<ApiResponse<ConsultationDto>>> GetByAppointmentId(int appointmentId)
    {
        try
        {
            var c = await _consultationService.GetByAppointmentIdAsync(appointmentId);
            if (c == null)
                return NotFound(ApiResponse<ConsultationDto>.ErrorResponse("Appointment or consultation not found"));
            return Ok(ApiResponse<ConsultationDto>.SuccessResponse(c, "Consultation retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving consultation for appointment {AppointmentId}", appointmentId);
            return StatusCode(500, ApiResponse<ConsultationDto>.ErrorResponse("An error occurred"));
        }
    }

    [HttpGet("by-pet/{petId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PreviousConsultSummaryDto>>>> GetPreviousConsults(int petId, [FromQuery] int excludeAppointmentId = 0)
    {
        try
        {
            var list = await _consultationService.GetPreviousConsultsByPetIdAsync(petId, excludeAppointmentId);
            return Ok(ApiResponse<IEnumerable<PreviousConsultSummaryDto>>.SuccessResponse(list, "Previous consults retrieved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving previous consults for pet {PetId}", petId);
            return StatusCode(500, ApiResponse<IEnumerable<PreviousConsultSummaryDto>>.ErrorResponse("An error occurred"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ConsultationDto>>> Save([FromBody] SaveConsultationDto dto)
    {
        try
        {
            if (dto == null)
                return BadRequest(ApiResponse<ConsultationDto>.ErrorResponse("Consultation data is required"));
            var result = await _consultationService.SaveAsync(dto);
            return Ok(ApiResponse<ConsultationDto>.SuccessResponse(result, "Consultation saved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving consultation");
            return StatusCode(500, ApiResponse<ConsultationDto>.ErrorResponse("An error occurred while saving consultation"));
        }
    }

    [HttpPost("{id}/prescription")]
    public async Task<ActionResult<ApiResponse<string>>> UploadPrescription(int id, IFormFile? file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<string>.ErrorResponse("No file uploaded"));
            var uploadsDir = Path.Combine(_env.ContentRootPath, "uploads", "prescriptions");
            Directory.CreateDirectory(uploadsDir);
            var ext = Path.GetExtension(file.FileName) ?? ".pdf";
            var fileName = $"consult_{id}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);
            await using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);
            var relativeUrl = $"uploads/prescriptions/{fileName}";
            var ok = await _consultationService.SetPrescriptionUrlAsync(id, relativeUrl);
            if (!ok)
                return NotFound(ApiResponse<string>.ErrorResponse("Consultation not found"));
            return Ok(ApiResponse<string>.SuccessResponse(relativeUrl, "Prescription uploaded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading prescription for consultation {Id}", id);
            return StatusCode(500, ApiResponse<string>.ErrorResponse("An error occurred while uploading prescription"));
        }
    }

    [HttpGet("prescription-file/{id}")]
    public async Task<IActionResult> GetPrescriptionFile(int id)
    {
        var c = await _consultationRepository.GetByIdAsync(id);
        if (c == null || string.IsNullOrEmpty(c.PrescriptionUrl))
            return NotFound();
        var physicalPath = Path.Combine(_env.ContentRootPath, c.PrescriptionUrl);
        if (!System.IO.File.Exists(physicalPath))
            return NotFound();
        var contentType = "application/pdf";
        if (physicalPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) contentType = "image/png";
        else if (physicalPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || physicalPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)) contentType = "image/jpeg";
        return PhysicalFile(physicalPath, contentType, Path.GetFileName(physicalPath));
    }
}
