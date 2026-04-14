using System.ComponentModel.DataAnnotations;

namespace PawNect.PetParent.Web.Models;

public class OtpChallengeViewModel
{
    public Guid ChallengeId { get; set; }
    public string MaskedDestination { get; set; } = string.Empty;
    public int ExpiresInSeconds { get; set; }
    public int ResendInSeconds { get; set; }
}

public class VerifyOtpViewModel
{
    [Required]
    public Guid ChallengeId { get; set; }

    [Required]
    public string Purpose { get; set; } = string.Empty;

    public string MaskedDestination { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter the OTP code.")]
    [StringLength(10, MinimumLength = 4)]
    public string OtpCode { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
