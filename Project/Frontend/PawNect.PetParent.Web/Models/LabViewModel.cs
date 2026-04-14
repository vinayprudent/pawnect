namespace PawNect.PetParent.Web.Models;

/// <summary>
/// Lab listing for Find Labs (registered labs from API).
/// </summary>
public class LabViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;       // Contact name
    public string LabName { get; set; } = string.Empty;     // Organization name
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string LocationDisplay => string.Join(", ", new[] { City, State }.Where(s => !string.IsNullOrWhiteSpace(s)));
}
