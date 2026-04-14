namespace PawNect.Domain.Entities;

/// <summary>
/// Single test line in a diagnostic order.
/// </summary>
public class DiagnosticOrderLine : BaseEntity
{
    public int DiagnosticOrderId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public int? LabTestCatalogItemId { get; set; }
    public decimal Price { get; set; }

    public DiagnosticOrder? DiagnosticOrder { get; set; }
}
