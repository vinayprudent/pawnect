namespace PawNect.Application.DTOs.Appointment;

/// <summary>
/// Appointment list item for vet dashboard or owner's appointments (includes pet/parent names).
/// </summary>
public class AppointmentListDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public string? SlotDate { get; set; }
    public string? SlotTime { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Booked";
    public string? ProviderName { get; set; }
    public string? Location { get; set; }
    public int? VetId { get; set; }
    // Owner (parent) info
    public int? OwnerId { get; set; }
    public string? ParentName { get; set; }
    public string? ParentEmail { get; set; }
    public string? ParentPhone { get; set; }
}
