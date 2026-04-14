namespace PawNect.Domain.Entities;

/// <summary>
/// Appointment entity for vet visits, training, grooming, etc.
/// </summary>
public class Appointment : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string AppointmentType { get; set; } = string.Empty; // Vet, Training, Grooming, etc.
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
    public string? ProviderName { get; set; }
    public string? ProviderContact { get; set; }
    public string? Notes { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCancelled { get; set; }
    /// <summary>Booked, In Progress, Completed, Closed, Cancelled</summary>
    public string Status { get; set; } = "Booked";

    // Foreign Keys
    public int PetId { get; set; }
    public int? VetId { get; set; }
    public int? OwnerId { get; set; }

    // Navigation
    public Pet? Pet { get; set; }
    public User? Vet { get; set; }
    public User? Owner { get; set; }
}
