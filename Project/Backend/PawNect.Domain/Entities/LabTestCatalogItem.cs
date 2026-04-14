namespace PawNect.Domain.Entities;

/// <summary>
/// Catalog of lab tests/packages that vets can order (e.g. CBC, LFT, Annual Wellness).
/// </summary>
public class LabTestCatalogItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string TestType { get; set; } = "Individual"; // Individual | Package
    public decimal Price { get; set; }
    public string? SampleType { get; set; }
    public string? Description { get; set; }
    /// <summary>For packages: JSON array of test names included.</summary>
    public string? TestsIncludedJson { get; set; }
}
