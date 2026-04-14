using Microsoft.AspNetCore.Mvc;
using PawNect.Application.DTOs;
using PawNect.Application.DTOs.Appointment;
using PawNect.Application.Interfaces;

namespace PawNect.API.Controllers;

/// <summary>
/// Appointments API Controller for creating and managing appointments (e.g. vet bookings by parents).
/// </summary>
[ApiController]
[Route("[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(IAppointmentService appointmentService, ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new appointment (e.g. when a parent confirms a vet booking).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AppointmentDto>>> CreateAppointment([FromBody] CreateAppointmentDto createAppointmentDto)
    {
        try
        {
            if (createAppointmentDto == null)
                return BadRequest(ApiResponse<AppointmentDto>.ErrorResponse("Appointment data is required"));

            if (createAppointmentDto.PetId <= 0)
                return BadRequest(ApiResponse<AppointmentDto>.ErrorResponse("Valid PetId is required"));

            if (string.IsNullOrWhiteSpace(createAppointmentDto.Title))
                return BadRequest(ApiResponse<AppointmentDto>.ErrorResponse("Title is required"));

            if (createAppointmentDto.EndTime <= createAppointmentDto.StartTime)
                return BadRequest(ApiResponse<AppointmentDto>.ErrorResponse("EndTime must be after StartTime"));

            var appointment = await _appointmentService.CreateAppointmentAsync(createAppointmentDto);
            return CreatedAtAction(nameof(GetAppointmentById), new { id = appointment.Id },
                ApiResponse<AppointmentDto>.SuccessResponse(appointment, "Appointment created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            return StatusCode(500, ApiResponse<AppointmentDto>.ErrorResponse("An error occurred while creating the appointment"));
        }
    }

    /// <summary>
    /// Get appointments for vet (dashboard: today + upcoming).
    /// </summary>
    [HttpGet("vet/{vetId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentListDto>>>> GetByVetId(int vetId, [FromQuery] DateTime? fromDate, [FromQuery] bool upcomingOnly = false)
    {
        try
        {
            var list = await _appointmentService.GetByVetIdAsync(vetId, fromDate, upcomingOnly);
            return Ok(ApiResponse<IEnumerable<AppointmentListDto>>.SuccessResponse(list, "Appointments retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments for vet {VetId}", vetId);
            return StatusCode(500, ApiResponse<IEnumerable<AppointmentListDto>>.ErrorResponse("An error occurred while retrieving appointments"));
        }
    }

    /// <summary>
    /// Get appointments for pet parent (owner).
    /// </summary>
    [HttpGet("owner/{ownerId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentListDto>>>> GetByOwnerId(int ownerId)
    {
        try
        {
            var list = await _appointmentService.GetByOwnerIdAsync(ownerId);
            return Ok(ApiResponse<IEnumerable<AppointmentListDto>>.SuccessResponse(list, "Appointments retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments for owner {OwnerId}", ownerId);
            return StatusCode(500, ApiResponse<IEnumerable<AppointmentListDto>>.ErrorResponse("An error occurred while retrieving appointments"));
        }
    }

    /// <summary>
    /// Get appointment by ID (used by CreatedAtAction).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AppointmentDto>>> GetAppointmentById(int id)
    {
        try
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null)
                return NotFound(ApiResponse<AppointmentDto>.ErrorResponse("Appointment not found"));

            return Ok(ApiResponse<AppointmentDto>.SuccessResponse(appointment, "Appointment retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointment {AppointmentId}", id);
            return StatusCode(500, ApiResponse<AppointmentDto>.ErrorResponse("An error occurred while retrieving the appointment"));
        }
    }

    /// <summary>
    /// Update an appointment (reschedule or cancel).
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AppointmentDto>>> UpdateAppointment(int id, [FromBody] UpdateAppointmentDto updateDto)
    {
        try
        {
            if (updateDto == null)
                return BadRequest(ApiResponse<AppointmentDto>.ErrorResponse("Update data is required"));

            if (updateDto.StartTime.HasValue && updateDto.EndTime.HasValue && updateDto.EndTime.Value <= updateDto.StartTime.Value)
                return BadRequest(ApiResponse<AppointmentDto>.ErrorResponse("EndTime must be after StartTime"));

            var appointment = await _appointmentService.UpdateAppointmentAsync(id, updateDto);
            if (appointment == null)
                return NotFound(ApiResponse<AppointmentDto>.ErrorResponse("Appointment not found"));

            return Ok(ApiResponse<AppointmentDto>.SuccessResponse(appointment, "Appointment updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment {AppointmentId}", id);
            return StatusCode(500, ApiResponse<AppointmentDto>.ErrorResponse("An error occurred while updating the appointment"));
        }
    }
}
