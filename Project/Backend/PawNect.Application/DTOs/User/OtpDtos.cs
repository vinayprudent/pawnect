namespace PawNect.Application.DTOs.User;

public class OtpChallengeResponseDto
{
    public Guid ChallengeId { get; set; }
    public string MaskedDestination { get; set; } = string.Empty;
    public int ExpiresInSeconds { get; set; }
    public int ResendInSeconds { get; set; }
}

public class RegisterVerifyOtpDto
{
    public Guid ChallengeId { get; set; }
    public string OtpCode { get; set; } = string.Empty;
}

public class LoginVerifyOtpDto
{
    public Guid ChallengeId { get; set; }
    public string OtpCode { get; set; } = string.Empty;
}

public class ResendOtpDto
{
    public Guid ChallengeId { get; set; }
}
