namespace PawNect.Domain.Entities;

/// <summary>
/// Medical Record entity storing pet health information
/// </summary>
public class MedicalRecord : BaseEntity
{
    public string RecordType { get; set; } = string.Empty; // Vaccination, Surgery, Checkup, etc.
    public DateTime RecordDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? VeterinarianName { get; set; }
    public string? ClinicName { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public string? FileUrl { get; set; }

    // Foreign Keys
    public int PetId { get; set; }

    // Navigation
    public Pet? Pet { get; set; }
}
