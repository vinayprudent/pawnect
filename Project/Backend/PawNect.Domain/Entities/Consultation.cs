namespace PawNect.Domain.Entities;

/// <summary>
/// Consultation (vet visit) linked to an appointment. Stores vitals, notes, diagnosis, prescription.
/// </summary>
public class Consultation : BaseEntity
{
    public int AppointmentId { get; set; }
    public int VetId { get; set; }
    public int PetId { get; set; }
    public int OwnerId { get; set; }
    public string Status { get; set; } = "Booked"; // Booked, In Progress, Completed, Closed
    public string? Notes { get; set; }
    public string? ProvisionalDiagnosis { get; set; }
    public string? PrescriptionUrl { get; set; }
    /// <summary>JSON array of { Name, Value, Unit, RecordedAt }</summary>
    public string? VitalsJson { get; set; }
    public bool ConsultationComplete { get; set; }
    public bool DiagnosticsRecommended { get; set; }

    public Appointment? Appointment { get; set; }
    public Pet? Pet { get; set; }
    public User? Owner { get; set; }
}
