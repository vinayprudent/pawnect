namespace PawNect.Application.DTOs.Diagnostic;

/// <summary>
/// Update diagnostic order status (Lab Flow — In-Clinic).
/// Allowed: Ordered, Sample Collected, Processing, Report Uploaded, Report Available, Reviewed.
/// </summary>
public class UpdateDiagnosticOrderStatusDto
{
    public string Status { get; set; } = string.Empty;
}
