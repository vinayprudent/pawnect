namespace PawNect.Application.DTOs.Appointment;

/// <summary>
/// DTO for reading appointment information.
/// </summary>
public class AppointmentDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AppointmentType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
    public string? ProviderName { get; set; }
    public string? ProviderContact { get; set; }
    public string? Notes { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCancelled { get; set; }
    public string Status { get; set; } = "Booked";
    public int? VetId { get; set; }
    public int? OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
