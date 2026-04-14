namespace PawNect.Application.DTOs.Diagnostic;

public class LabTestCatalogItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TestType { get; set; } = "Individual";
    public decimal Price { get; set; }
    public string? SampleType { get; set; }
    public string? Description { get; set; }
    public string? TestsIncludedJson { get; set; }
}
