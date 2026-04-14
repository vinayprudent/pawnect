using Microsoft.AspNetCore.Mvc;
using PawNect.Application.DTOs;
using PawNect.Application.DTOs.Pet;
using PawNect.Application.Interfaces;

namespace PawNect.API.Controllers;

/// <summary>
/// Pet API Controller for managing pets
/// </summary>
[ApiController]
[Route("[controller]")]
public class PetsController : ControllerBase
{
    private readonly IPetService _petService;
    private readonly ILogger<PetsController> _logger;

    public PetsController(IPetService petService, ILogger<PetsController> logger)
    {
        _petService = petService;
        _logger = logger;
    }

    /// <summary>
    /// Get all pets
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PetDto>>>> GetAllPets()
    {
        try
        {
            var pets = await _petService.GetAllPetsAsync();
            return Ok(ApiResponse<IEnumerable<PetDto>>.SuccessResponse(pets, "Pets retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pets");
            return StatusCode(500, ApiResponse<IEnumerable<PetDto>>.ErrorResponse("An error occurred while retrieving pets"));
        }
    }

    /// <summary>
    /// Get pet by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PetDto>>> GetPetById(int id)
    {
        try
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
                return NotFound(ApiResponse<PetDto>.ErrorResponse("Pet not found"));

            return Ok(ApiResponse<PetDto>.SuccessResponse(pet, "Pet retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pet {PetId}", id);
            return StatusCode(500, ApiResponse<PetDto>.ErrorResponse("An error occurred while retrieving the pet"));
        }
    }

    /// <summary>
    /// Get pets by owner ID
    /// </summary>
    [HttpGet("owner/{ownerId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PetDto>>>> GetPetsByOwner(int ownerId)
    {
        try
        {
            var pets = await _petService.GetPetsByOwnerAsync(ownerId);
            return Ok(ApiResponse<IEnumerable<PetDto>>.SuccessResponse(pets, "Owner's pets retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pets for owner {OwnerId}", ownerId);
            return StatusCode(500, ApiResponse<IEnumerable<PetDto>>.ErrorResponse("An error occurred while retrieving pets"));
        }
    }

    /// <summary>
    /// Create a new pet
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PetDto>>> CreatePet([FromBody] CreatePetDto createPetDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<PetDto>.ErrorResponse("Invalid pet data"));

            var pet = await _petService.CreatePetAsync(createPetDto);
            return CreatedAtAction(nameof(GetPetById), new { id = pet.Id }, 
                ApiResponse<PetDto>.SuccessResponse(pet, "Pet created successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating pet");
            return BadRequest(ApiResponse<PetDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pet");
            return StatusCode(500, ApiResponse<PetDto>.ErrorResponse("An error occurred while creating the pet"));
        }
    }

    /// <summary>
    /// Update an existing pet
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<PetDto>>> UpdatePet(int id, [FromBody] UpdatePetDto updatePetDto)
    {
        try
        {
            // Allow clients to omit Id in body; route id is authoritative
            if (updatePetDto.Id == 0)
                updatePetDto.Id = id;

            if (id != updatePetDto.Id)
                return BadRequest(ApiResponse<PetDto>.ErrorResponse("ID mismatch"));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<PetDto>.ErrorResponse("Invalid pet data"));

            var pet = await _petService.UpdatePetAsync(updatePetDto);
            return Ok(ApiResponse<PetDto>.SuccessResponse(pet, "Pet updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Pet not found for update");
            return NotFound(ApiResponse<PetDto>.ErrorResponse(ex.Message));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating pet");
            return BadRequest(ApiResponse<PetDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating pet");
            return StatusCode(500, ApiResponse<PetDto>.ErrorResponse("An error occurred while updating the pet"));
        }
    }

    /// <summary>
    /// Delete a pet
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeletePet(int id)
    {
        try
        {
            var success = await _petService.DeletePetAsync(id);
            if (!success)
                return NotFound(ApiResponse.ErrorResponse("Pet not found"));

            return Ok(ApiResponse.SuccessResponse("Pet deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting pet {PetId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting the pet"));
        }
    }
}
