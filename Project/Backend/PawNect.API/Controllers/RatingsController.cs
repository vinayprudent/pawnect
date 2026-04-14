using Microsoft.AspNetCore.Mvc;
using PawNect.Application.DTOs;
using PawNect.Application.DTOs.Rating;
using PawNect.Application.Interfaces;

namespace PawNect.API.Controllers;

/// <summary>
/// API for vet ratings (parent rates vet) and parent ratings (vet rates parent). Stored in database.
/// </summary>
[ApiController]
[Route("[controller]")]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;
    private readonly ILogger<RatingsController> _logger;

    public RatingsController(IRatingService ratingService, ILogger<RatingsController> logger)
    {
        _ratingService = ratingService;
        _logger = logger;
    }

    /// <summary>Get average rating for a vet (from parent ratings).</summary>
    [HttpGet("vet/{vetId}/average")]
    public async Task<ActionResult<ApiResponse<double>>> GetVetAverageRating(int vetId)
    {
        try
        {
            var average = await _ratingService.GetAverageRatingForVetAsync(vetId);
            return Ok(ApiResponse<double>.SuccessResponse(average, "Vet average rating retrieved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vet average rating for VetId {VetId}", vetId);
            return StatusCode(500, ApiResponse<double>.ErrorResponse("An error occurred while retrieving the rating"));
        }
    }

    /// <summary>Check if parent has already rated a booking (vet rating).</summary>
    [HttpGet("vet/has-rated")]
    public async Task<ActionResult<ApiResponse<bool>>> HasParentRatedBooking([FromQuery] int parentUserId, [FromQuery] string bookingId)
    {
        try
        {
            var hasRated = await _ratingService.HasParentRatedBookingAsync(parentUserId, bookingId);
            return Ok(ApiResponse<bool>.SuccessResponse(hasRated, "Status retrieved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking has-rated for ParentUserId {ParentUserId}, BookingId {BookingId}", parentUserId, bookingId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred"));
        }
    }

    /// <summary>Submit or update parent's rating of a vet.</summary>
    [HttpPost("vet")]
    public async Task<ActionResult<ApiResponse>> SubmitVetRating([FromBody] CreateVetRatingDto dto)
    {
        try
        {
            if (dto.VetId <= 0 || dto.ParentUserId <= 0 || string.IsNullOrWhiteSpace(dto.BookingId))
                return BadRequest(ApiResponse.ErrorResponse("VetId, ParentUserId, and BookingId are required"));
            await _ratingService.SubmitVetRatingAsync(dto);
            return Ok(ApiResponse.SuccessResponse("Rating saved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting vet rating");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while saving the rating"));
        }
    }

    /// <summary>Get vet's rating of a pet parent for a booking.</summary>
    [HttpGet("parent")]
    public async Task<ActionResult<ApiResponse<ParentRatingDto?>>> GetParentRating([FromQuery] int vetId, [FromQuery] string bookingId)
    {
        try
        {
            var rating = await _ratingService.GetParentRatingByBookingAsync(vetId, bookingId);
            return Ok(ApiResponse<ParentRatingDto?>.SuccessResponse(rating, "Parent rating retrieved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parent rating for VetId {VetId}, BookingId {BookingId}", vetId, bookingId);
            return StatusCode(500, ApiResponse<ParentRatingDto?>.ErrorResponse("An error occurred"));
        }
    }

    /// <summary>Submit or update vet's rating of a pet parent.</summary>
    [HttpPost("parent")]
    public async Task<ActionResult<ApiResponse>> SubmitParentRating([FromBody] CreateParentRatingDto dto)
    {
        try
        {
            if (dto.VetId <= 0 || dto.ParentUserId <= 0 || string.IsNullOrWhiteSpace(dto.BookingId))
                return BadRequest(ApiResponse.ErrorResponse("VetId, ParentUserId, and BookingId are required"));
            await _ratingService.SubmitParentRatingAsync(dto);
            return Ok(ApiResponse.SuccessResponse("Rating saved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting parent rating");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while saving the rating"));
        }
    }
}
