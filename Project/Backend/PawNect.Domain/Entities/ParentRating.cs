namespace PawNect.Domain.Entities;

/// <summary>
/// Rating given by a vet to a pet parent (1-5 stars, optional comment).
/// Keyed by booking reference (frontend BookingId).
/// </summary>
public class ParentRating : BaseEntity
{
    public int ParentUserId { get; set; }
    public int VetId { get; set; }
    public string BookingId { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
}
