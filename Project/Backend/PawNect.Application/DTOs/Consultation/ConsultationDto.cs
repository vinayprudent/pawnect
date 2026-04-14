namespace PawNect.Application.DTOs.Consultation;

public class ConsultationDto
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int VetId { get; set; }
    public int PetId { get; set; }
    public int OwnerId { get; set; }
    public string Status { get; set; } = "Booked";
    public string? Notes { get; set; }
    public string? ProvisionalDiagnosis { get; set; }
    public string? PrescriptionUrl { get; set; }
    public string? VitalsJson { get; set; }
    public bool ConsultationComplete { get; set; }
    public bool DiagnosticsRecommended { get; set; }
    // From appointment
    public string? PetName { get; set; }
    public string? PetSpecies { get; set; }
    public string? PetBreed { get; set; }
    public double? PetWeightKg { get; set; }
    public string? ParentName { get; set; }
    public string? ParentEmail { get; set; }
    public string? ParentPhone { get; set; }
    public string? ReasonForVisit { get; set; }
    public string? VetName { get; set; }
    public string? ClinicName { get; set; }
    public string? ClinicAddress { get; set; }
    public DateTime? SlotStart { get; set; }
}
