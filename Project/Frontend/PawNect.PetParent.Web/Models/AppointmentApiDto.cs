namespace PawNect.PetParent.Web.Models;

/// <summary>Appointment list item from API (owner or vet list).</summary>
public class AppointmentListApiDto
{
    public int Id { get; set; }
    public string? PetName { get; set; }
    public string? SlotDate { get; set; }
    public string? SlotTime { get; set; }
    public string? Status { get; set; }
    public string? ProviderName { get; set; }
    public string? Location { get; set; }
    public int? VetId { get; set; }
    public int? OwnerId { get; set; }
    public string? ParentName { get; set; }
}

/// <summary>Single appointment from API (GET by id).</summary>
public class AppointmentApiDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public int? VetId { get; set; }
    public int? OwnerId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? ProviderName { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }
}
