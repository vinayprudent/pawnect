using PawNect.Application.DTOs.Rating;
using PawNect.Application.Interfaces;
using PawNect.Domain.Entities;

namespace PawNect.Application.Services;

/// <summary>
/// Service for vet and parent ratings (database-backed).
/// </summary>
public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepository;

    public RatingService(IRatingRepository ratingRepository)
    {
        _ratingRepository = ratingRepository;
    }

    public async Task<double> GetAverageRatingForVetAsync(int vetId)
    {
        var ratings = await _ratingRepository.GetVetRatingsByVetIdAsync(vetId);
        var list = ratings.ToList();
        if (list.Count == 0) return 0;
        return list.Average(r => r.Rating);
    }

    public async Task<int> GetVetRatingCountAsync(int vetId)
    {
        var ratings = await _ratingRepository.GetVetRatingsByVetIdAsync(vetId);
        return ratings.Count();
    }

    public async Task<bool> HasParentRatedBookingAsync(int parentUserId, string bookingId)
    {
        var existing = await _ratingRepository.GetVetRatingByParentAndBookingAsync(parentUserId, bookingId);
        return existing != null;
    }

    public async Task SubmitVetRatingAsync(CreateVetRatingDto dto)
    {
        var rating = Math.Clamp(dto.Rating, 1, 5);
        var entity = new VetRating
        {
            VetId = dto.VetId,
            ParentUserId = dto.ParentUserId,
            BookingId = dto.BookingId,
            Rating = rating,
            Comment = dto.Comment
        };
        await _ratingRepository.AddOrUpdateVetRatingAsync(entity);
        await _ratingRepository.SaveChangesAsync();
    }

    public async Task<ParentRatingDto?> GetParentRatingByBookingAsync(int vetId, string bookingId)
    {
        var existing = await _ratingRepository.GetParentRatingByVetAndBookingAsync(vetId, bookingId);
        if (existing == null) return null;
        return new ParentRatingDto
        {
            Id = existing.Id,
            ParentUserId = existing.ParentUserId,
            VetId = existing.VetId,
            BookingId = existing.BookingId,
            Rating = existing.Rating,
            Comment = existing.Comment,
            CreatedAt = existing.CreatedAt
        };
    }

    public async Task SubmitParentRatingAsync(CreateParentRatingDto dto)
    {
        var rating = Math.Clamp(dto.Rating, 1, 5);
        var entity = new ParentRating
        {
            ParentUserId = dto.ParentUserId,
            VetId = dto.VetId,
            BookingId = dto.BookingId,
            Rating = rating,
            Comment = dto.Comment
        };
        await _ratingRepository.AddOrUpdateParentRatingAsync(entity);
        await _ratingRepository.SaveChangesAsync();
    }
}
