namespace PawNect.Application.DTOs.Rating;

/// <summary>Request to add or update a vet's rating of a pet parent.</summary>
public class CreateParentRatingDto
{
    public int ParentUserId { get; set; }
    public int VetId { get; set; }
    public string BookingId { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
}
