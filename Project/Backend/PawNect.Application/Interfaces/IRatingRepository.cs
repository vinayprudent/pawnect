using PawNect.Domain.Entities;

namespace PawNect.Application.Interfaces;

/// <summary>
/// Repository for vet and parent ratings.
/// </summary>
public interface IRatingRepository
{
    Task<VetRating?> GetVetRatingAsync(int vetId, int parentUserId, string bookingId);
    Task<VetRating?> GetVetRatingByParentAndBookingAsync(int parentUserId, string bookingId);
    Task<IEnumerable<VetRating>> GetVetRatingsByVetIdAsync(int vetId);
    Task<VetRating> AddOrUpdateVetRatingAsync(VetRating rating);
    Task<ParentRating?> GetParentRatingAsync(int parentUserId, int vetId, string bookingId);
    Task<ParentRating?> GetParentRatingByVetAndBookingAsync(int vetId, string bookingId);
    Task<ParentRating> AddOrUpdateParentRatingAsync(ParentRating rating);
    Task SaveChangesAsync();
}
