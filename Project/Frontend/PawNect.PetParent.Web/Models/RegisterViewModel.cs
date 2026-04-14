using System.ComponentModel.DataAnnotations;

namespace PawNect.PetParent.Web.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [StringLength(50)]
    [Display(Name = "City")]
    public string? City { get; set; }

    [StringLength(50)]
    [Display(Name = "State")]
    public string? State { get; set; }

    [StringLength(20)]
    [Display(Name = "Zip Code")]
    public string? ZipCode { get; set; }
}
