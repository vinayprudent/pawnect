namespace PawNect.PetParent.Web.Models;

/// <summary>API response shape for consultation.</summary>
public class ConsultationApiDto
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

public class VitalEntryDto
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? RecordedAt { get; set; }
}

public class PreviousConsultApiDto
{
    public string Date { get; set; } = string.Empty;
    public string VetName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}

public class ParentRatingApiDto
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class SelectedTestDto
{
    public int TestId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class LabTestCatalogItemApiDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TestType { get; set; } = "Individual";
    public decimal Price { get; set; }
    public string? SampleType { get; set; }
    public string? Description { get; set; }
}

public class DiagnosticOrderApiDto
{
    public int Id { get; set; }
    public string Status { get; set; } = "Ordered";
    public decimal TotalPrice { get; set; }
    public string? AssignedLabName { get; set; }
    public DateTime? OrderedAt { get; set; }
    public DateTime? SampleCollectedAt { get; set; }
    public DateTime? ReportUploadedAt { get; set; }
    public List<string>? TestsOrdered { get; set; }
}

public class DiagnosticReportApiDto
{
    public int Id { get; set; }
    public int DiagnosticOrderId { get; set; }
    public string? ReportFileUrl { get; set; }
    public string? ReportFileName { get; set; }
    public string? VetAdvice { get; set; }
    public string? NextSteps { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
