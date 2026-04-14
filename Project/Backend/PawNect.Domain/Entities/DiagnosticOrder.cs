namespace PawNect.Domain.Entities;

/// <summary>
/// Diagnostic/lab order placed by vet during consultation (in-clinic collection).
/// </summary>
public class DiagnosticOrder : BaseEntity
{
    public int ConsultationId { get; set; }
    public int PetId { get; set; }
    public int OwnerId { get; set; }
    public int VetId { get; set; }
    /// <summary>Ordered, Sample Collected, Processing, Report Uploaded, Report Available, Reviewed</summary>
    public string Status { get; set; } = "Ordered";
    public string CollectionType { get; set; } = "In-Clinic";
    public string? AssignedLabName { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime? OrderedAt { get; set; }
    public DateTime? SampleCollectedAt { get; set; }
    public DateTime? ReportUploadedAt { get; set; }

    public Consultation? Consultation { get; set; }
    public ICollection<DiagnosticOrderLine> Lines { get; set; } = new List<DiagnosticOrderLine>();
}
