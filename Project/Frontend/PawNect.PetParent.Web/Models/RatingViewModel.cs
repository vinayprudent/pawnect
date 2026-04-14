namespace PawNect.PetParent.Web.Models;

/// <summary>Parent rates vet after a visit (1-5 stars, optional comment).</summary>
public class VetRatingViewModel
{
    public int VetId { get; set; }
    public int ParentUserId { get; set; }
    public string BookingId { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
}

/// <summary>Vet rates pet parent (1-5 stars, optional comment).</summary>
public class ParentRatingViewModel
{
    public int ParentUserId { get; set; }
    public int VetId { get; set; }
    public string BookingId { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
}
