namespace PawNect.Application.DTOs.Diagnostic;

public class CreateDiagnosticOrderDto
{
    public int ConsultationId { get; set; }
    public int PetId { get; set; }
    public int OwnerId { get; set; }
    public int VetId { get; set; }
    public string CollectionType { get; set; } = "In-Clinic";
    public string? AssignedLabName { get; set; }
    public List<DiagnosticOrderLineDto> Tests { get; set; } = new();
}

public class DiagnosticOrderLineDto
{
    public string TestName { get; set; } = string.Empty;
    public int? LabTestCatalogItemId { get; set; }
    public decimal Price { get; set; }
}
