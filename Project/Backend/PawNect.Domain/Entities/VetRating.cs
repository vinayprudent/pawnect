namespace PawNect.Domain.Entities;

/// <summary>
/// Rating given by a pet parent to a vet after a visit (1-5 stars, optional comment).
/// Keyed by booking reference (frontend BookingId).
/// </summary>
public class VetRating : BaseEntity
{
    public int VetId { get; set; }
    public int ParentUserId { get; set; }
    public string BookingId { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
}
