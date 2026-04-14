using PawNect.Domain.Enums;

namespace PawNect.Domain.Entities;

/// <summary>
/// Pet entity representing a pet owned by a user
/// </summary>
public class Pet : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public PetSpecies Species { get; set; }
    public DateTime DateOfBirth { get; set; }
    public double? WeightKg { get; set; }
    public string? Color { get; set; }
    public string? MicrochipId { get; set; }
    public PetStatus Status { get; set; } = PetStatus.Active;
    public string? ProfileImageUrl { get; set; }
    public string? Description { get; set; }

    // Foreign Keys
    public int OwnerId { get; set; }

    // Navigation
    public User? Owner { get; set; }
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
