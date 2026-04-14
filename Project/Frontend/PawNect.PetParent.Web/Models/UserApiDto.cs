namespace PawNect.PetParent.Web.Models;

/// <summary>
/// User DTO from API (GET users/byrole, GET users/{id})
/// </summary>
public class UserApiDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int Role { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? OrganizationName { get; set; }
}
