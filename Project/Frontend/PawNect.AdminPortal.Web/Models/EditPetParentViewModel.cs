using System.ComponentModel.DataAnnotations;

namespace PawNect.AdminPortal.Web.Models;

public class EditPetParentViewModel
{
    public int Id { get; set; }

    [Required, Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone, Display(Name = "Phone")]
    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
}
