using PawNect.Domain.Enums;

namespace PawNect.Domain.Entities;

public class OtpChallenge : BaseEntity
{
    public Guid ChallengeId { get; set; } = Guid.NewGuid();
    public OtpPurpose Purpose { get; set; }
    public OtpChannel Channel { get; set; } = OtpChannel.Email;
    public string Destination { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public User? User { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public DateTime ResendAvailableAt { get; set; }
    public string? MetadataJson { get; set; }
}
