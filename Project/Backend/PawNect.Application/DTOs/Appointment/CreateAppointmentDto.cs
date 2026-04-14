namespace PawNect.Application.DTOs.Appointment;

/// <summary>
/// DTO for creating a new appointment (e.g. vet booking by parent).
/// </summary>
public class CreateAppointmentDto
{
    public int PetId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AppointmentType { get; set; } = "Vet";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
    public string? ProviderName { get; set; }
    public string? ProviderContact { get; set; }
    public string? Notes { get; set; }
    public int? VetId { get; set; }
    public int? OwnerId { get; set; }
}
