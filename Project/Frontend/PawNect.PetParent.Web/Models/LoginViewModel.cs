using System.ComponentModel.DataAnnotations;

namespace PawNect.PetParent.Web.Models;

/// <summary>
/// OTP login using email or mobile number.
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Email or mobile number is required")]
    [Display(Name = "Email or Mobile Number")]
    public string EmailOrMobile { get; set; } = string.Empty;

}
