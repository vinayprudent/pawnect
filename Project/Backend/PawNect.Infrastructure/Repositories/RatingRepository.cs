using Microsoft.EntityFrameworkCore;
using PawNect.Application.Interfaces;
using PawNect.Domain.Entities;
using PawNect.Infrastructure.DbContext;

namespace PawNect.Infrastructure.Repositories;

/// <summary>
/// Repository for vet and parent ratings.
/// </summary>
public class RatingRepository : IRatingRepository
{
    private readonly PawNectDbContext _context;

    public RatingRepository(PawNectDbContext context)
    {
        _context = context;
    }

    public async Task<VetRating?> GetVetRatingAsync(int vetId, int parentUserId, string bookingId)
    {
        return await _context.VetRatings
            .FirstOrDefaultAsync(r => r.VetId == vetId && r.ParentUserId == parentUserId && r.BookingId == bookingId && !r.IsDeleted);
    }

    public async Task<VetRating?> GetVetRatingByParentAndBookingAsync(int parentUserId, string bookingId)
    {
        return await _context.VetRatings
            .FirstOrDefaultAsync(r => r.ParentUserId == parentUserId && r.BookingId == bookingId && !r.IsDeleted);
    }

    public async Task<IEnumerable<VetRating>> GetVetRatingsByVetIdAsync(int vetId)
    {
        return await _context.VetRatings
            .Where(r => r.VetId == vetId && !r.IsDeleted)
            .ToListAsync();
    }

    public async Task<VetRating> AddOrUpdateVetRatingAsync(VetRating rating)
    {
        var existing = await GetVetRatingAsync(rating.VetId, rating.ParentUserId, rating.BookingId);
        if (existing != null)
        {
            existing.Rating = rating.Rating;
            existing.Comment = rating.Comment;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.VetRatings.Update(existing);
            return existing;
        }
        await _context.VetRatings.AddAsync(rating);
        return rating;
    }

    public async Task<ParentRating?> GetParentRatingAsync(int parentUserId, int vetId, string bookingId)
    {
        return await _context.ParentRatings
            .FirstOrDefaultAsync(r => r.ParentUserId == parentUserId && r.VetId == vetId && r.BookingId == bookingId && !r.IsDeleted);
    }

    public async Task<ParentRating?> GetParentRatingByVetAndBookingAsync(int vetId, string bookingId)
    {
        return await _context.ParentRatings
            .FirstOrDefaultAsync(r => r.VetId == vetId && r.BookingId == bookingId && !r.IsDeleted);
    }

    public async Task<ParentRating> AddOrUpdateParentRatingAsync(ParentRating rating)
    {
        var existing = await GetParentRatingAsync(rating.ParentUserId, rating.VetId, rating.BookingId);
        if (existing != null)
        {
            existing.Rating = rating.Rating;
            existing.Comment = rating.Comment;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.ParentRatings.Update(existing);
            return existing;
        }
        await _context.ParentRatings.AddAsync(rating);
        return rating;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
