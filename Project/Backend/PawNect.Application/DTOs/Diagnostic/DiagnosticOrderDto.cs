namespace PawNect.Application.DTOs.Diagnostic;

public class DiagnosticOrderDto
{
    public int Id { get; set; }
    public int ConsultationId { get; set; }
    public int PetId { get; set; }
    public int OwnerId { get; set; }
    public int VetId { get; set; }
    public string Status { get; set; } = "Ordered";
    public string CollectionType { get; set; } = "In-Clinic";
    public string? AssignedLabName { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime? OrderedAt { get; set; }
    public DateTime? SampleCollectedAt { get; set; }
    public DateTime? ReportUploadedAt { get; set; }
    public List<string> TestsOrdered { get; set; } = new();
}
