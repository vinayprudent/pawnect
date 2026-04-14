namespace PawNect.PetParent.Web.Models;

/// <summary>
/// Consult status: Booked → In Progress → Completed → Closed
/// </summary>
public static class ConsultStatus
{
    public const string Booked = "Booked";
    public const string InProgress = "In Progress";
    public const string Completed = "Completed";
    public const string Closed = "Closed";
}

/// <summary>
/// Diagnostics status: Recommended → Ordered → Sample Collected → Processing → Report Uploaded → Report Available → Reviewed
/// </summary>
public static class DiagnosticStatus
{
    public const string Recommended = "Recommended";
    public const string Ordered = "Ordered";
    public const string SampleCollected = "Sample Collected";
    public const string Processing = "Processing";
    public const string ReportUploaded = "Report Uploaded";
    public const string ReportAvailable = "Report Available";
    public const string Reviewed = "Reviewed";
}

/// <summary>
/// Pet & parent details + reason for visit + previous consults (Vet Doctor Flow – Pet Parent view)
/// </summary>
public class ConsultationDetailsViewModel
{
    public string BookingId { get; set; } = string.Empty;
    public int ConsultationId { get; set; }
    public string ConsultStatus { get; set; } = "Booked";

    // Pet & parent
    public string PetName { get; set; } = string.Empty;
    public string PetSpecies { get; set; } = string.Empty;
    public string? PetBreed { get; set; }
    public double? PetWeightKg { get; set; }
    public string? ParentName { get; set; }
    public string? ParentPhone { get; set; }
    public string? ParentEmail { get; set; }

    // Visit
    public int? VetId { get; set; }
    public int? ParentUserId { get; set; }
    public int? PetId { get; set; }
    public int? OwnerId { get; set; }
    public string VetName { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public string ClinicAddress { get; set; } = string.Empty;
    public string SlotDate { get; set; } = string.Empty;
    public string SlotTime { get; set; } = string.Empty;
    public string ReasonForVisit { get; set; } = string.Empty;

    // Previous consults (summary)
    public List<PreviousConsultSummaryViewModel> PreviousConsults { get; set; } = new();

    // Vet-added (Pet Parent sees)
    public List<VitalViewModel> Vitals { get; set; } = new();
    public string? Notes { get; set; }
    public string? ProvisionalDiagnosis { get; set; }
    public string? PrescriptionUrl { get; set; }
    public string? PrescriptionFileName { get; set; }

    // Next actions (reflected from vet)
    public bool ConsultationComplete { get; set; }
    public bool DiagnosticsRecommended { get; set; }

    // Diagnostics (when recommended)
    public DiagnosticOrderViewModel? DiagnosticOrder { get; set; }

    // Report (when available)
    public LabReportViewModel? Report { get; set; }
}

public class PreviousConsultSummaryViewModel
{
    public string Date { get; set; } = string.Empty;
    public string VetName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}

public class VitalViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? RecordedAt { get; set; }
}

/// <summary>
/// Diagnostics — In-Clinic: tests ordered, single price, assigned lab, status (Pet Parent sees)
/// </summary>
public class DiagnosticOrderViewModel
{
    public string OrderId { get; set; } = string.Empty;
    public string Status { get; set; } = DiagnosticStatus.Recommended;
    public List<string> TestsOrdered { get; set; } = new();
    public string CollectionType { get; set; } = "In-Clinic";
    public decimal TotalPrice { get; set; }
    public string? AssignedLab { get; set; }
    public string? OrderedAt { get; set; }
    public string? SampleCollectedAt { get; set; }
    public string? ReportUploadedAt { get; set; }
}

/// <summary>
/// Report flow: vet review + advice; pet parent sees report + next steps
/// </summary>
public class LabReportViewModel
{
    public string ReportId { get; set; } = string.Empty;
    public string? ReportUrl { get; set; }
    public string? ReportFileName { get; set; }
    public string? VetAdvice { get; set; }
    public string? NextSteps { get; set; }
    public string? ReviewedAt { get; set; }
}

/// <summary>
/// Vet consult screen: form model for adding vitals, notes, diagnosis, prescription, next actions.
/// </summary>
public class VetConsultUpdateViewModel
{
    public string BookingId { get; set; } = string.Empty;
    public int VetId { get; set; }
    public int PetId { get; set; }
    public int OwnerId { get; set; }
    public string? Notes { get; set; }
    public string? ProvisionalDiagnosis { get; set; }
    public string? PrescriptionFileName { get; set; }
    public bool ConsultationComplete { get; set; }
    public bool DiagnosticsRecommended { get; set; }
    /// <summary>Next action: In Progress, Completed, or empty to only save fields.</summary>
    public string? NextAction { get; set; }
    /// <summary>New vitals: "Name|Value|Unit" per line, or use VitalNames, VitalValues, VitalUnits.</summary>
    public string? NewVitals { get; set; }
}
