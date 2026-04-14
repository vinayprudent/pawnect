namespace PawNect.Application.DTOs.Diagnostic;

public class DiagnosticReportDto
{
    public int Id { get; set; }
    public int DiagnosticOrderId { get; set; }
    public string? ReportFileUrl { get; set; }
    public string? ReportFileName { get; set; }
    public string? VetAdvice { get; set; }
    public string? NextSteps { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
