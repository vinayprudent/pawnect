namespace PawNect.PetParent.Web.Models;

/// <summary>
/// Vet listing card (A2) and profile (A3)
/// </summary>
public class VetViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public string ClinicAddress { get; set; } = string.Empty;
    public string? ClinicLocation { get; set; }
    public int ExperienceYears { get; set; }
    public decimal ConsultationFee { get; set; }
    public double Rating { get; set; }
    public string AvailabilityIndicator { get; set; } = string.Empty; // "Today", "Tomorrow", "This week"
    public bool AvailableToday { get; set; }
    public bool AvailableTomorrow { get; set; }

    // Profile page (A3)
    public string? Bio { get; set; }
    public string? Qualifications { get; set; }
    public string? AreasOfExpertise { get; set; }
    public string? PracticeRegistrationNumber { get; set; }
    public string? TypicalCasesHandled { get; set; }
    public string? WeeklyAvailability { get; set; }
}
