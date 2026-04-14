using PawNect.Application.DTOs.Rating;

namespace PawNect.Application.Interfaces;

/// <summary>
/// Service for vet and parent ratings (persisted in database).
/// </summary>
public interface IRatingService
{
    Task<double> GetAverageRatingForVetAsync(int vetId);
    Task<int> GetVetRatingCountAsync(int vetId);
    Task<bool> HasParentRatedBookingAsync(int parentUserId, string bookingId);
    Task SubmitVetRatingAsync(CreateVetRatingDto dto);
    Task<ParentRatingDto?> GetParentRatingByBookingAsync(int vetId, string bookingId);
    Task SubmitParentRatingAsync(CreateParentRatingDto dto);
}
