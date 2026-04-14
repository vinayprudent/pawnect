using PawNect.Domain.Enums;

namespace PawNect.Domain.Entities;

/// <summary>
/// User entity representing a system user (Pet Parent, Clinic, Trainer, etc.)
/// </summary>
public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    /// <summary>Optional business/clinic name for VeterinaryClinic or Laboratory roles.</summary>
    public string? OrganizationName { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public ICollection<Pet> OwnedPets { get; set; } = new List<Pet>();
}
