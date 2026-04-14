namespace PawNect.Application.DTOs.Consultation;

public class SaveConsultationDto
{
    public int AppointmentId { get; set; }
    public int VetId { get; set; }
    public int PetId { get; set; }
    public int OwnerId { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public string? ProvisionalDiagnosis { get; set; }
    public string? PrescriptionUrl { get; set; }
    public string? VitalsJson { get; set; }
    public bool ConsultationComplete { get; set; }
    public bool DiagnosticsRecommended { get; set; }
}
