namespace PawNect.Application.DTOs.Rating;

/// <summary>Vet's rating of a pet parent (for display).</summary>
public class ParentRatingDto
{
    public int Id { get; set; }
    public int ParentUserId { get; set; }
    public int VetId { get; set; }
    public string BookingId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
