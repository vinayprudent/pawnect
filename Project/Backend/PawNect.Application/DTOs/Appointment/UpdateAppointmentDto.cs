namespace PawNect.Application.DTOs.Appointment;

/// <summary>
/// DTO for updating an appointment (reschedule or cancel).
/// </summary>
public class UpdateAppointmentDto
{
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool? IsCancelled { get; set; }
    public string? Status { get; set; }
}
