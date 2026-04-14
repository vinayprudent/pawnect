namespace PawNect.Application.DTOs.Diagnostic;

public class SaveDiagnosticReportDto
{
    public int DiagnosticOrderId { get; set; }
    public string? VetAdvice { get; set; }
    public string? NextSteps { get; set; }
}
