using System.ComponentModel.DataAnnotations;

namespace PawNect.AdminPortal.Web.Models;

public class RegisterVetViewModel
{
    [Required, Display(Name = "Clinic / Business name")]
    public string ClinicName { get; set; } = string.Empty;

    [Required, Display(Name = "Contact first name")]
    public string ContactFirstName { get; set; } = string.Empty;

    [Required, Display(Name = "Contact last name")]
    public string ContactLastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone, Display(Name = "Phone")]
    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
}
