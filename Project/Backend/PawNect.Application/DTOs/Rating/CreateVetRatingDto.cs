namespace PawNect.Application.DTOs.Rating;

/// <summary>Request to add or update a parent's rating of a vet.</summary>
public class CreateVetRatingDto
{
    public int VetId { get; set; }
    public int ParentUserId { get; set; }
    public string BookingId { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
}
