namespace PawNect.Domain.Entities;

/// <summary>
/// Report flow: vet uploads report and adds advice; pet parent sees report + next steps.
/// One report per diagnostic order.
/// </summary>
public class DiagnosticReport : BaseEntity
{
    public int DiagnosticOrderId { get; set; }
    public string? ReportFileUrl { get; set; }
    public string? ReportFileName { get; set; }
    public string? VetAdvice { get; set; }
    public string? NextSteps { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public DiagnosticOrder? DiagnosticOrder { get; set; }
}
