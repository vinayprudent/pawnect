namespace PawNect.Application.DTOs.User;

/// <summary>
/// DTO for user registration
/// </summary>
public class RegisterUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int Role { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    /// <summary>Optional. Business/Clinic name for Vet or Lab registration.</summary>
    public string? OrganizationName { get; set; }
}
