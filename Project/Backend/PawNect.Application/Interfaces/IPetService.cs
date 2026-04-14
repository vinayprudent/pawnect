using PawNect.Application.DTOs.Pet;

namespace PawNect.Application.Interfaces;

/// <summary>
/// Pet Service interface
/// </summary>
public interface IPetService
{
    Task<PetDto?> GetPetByIdAsync(int petId);
    Task<IEnumerable<PetDto>> GetAllPetsAsync();
    Task<IEnumerable<PetDto>> GetPetsByOwnerAsync(int ownerId);
    Task<PetDto> CreatePetAsync(CreatePetDto createPetDto);
    Task<PetDto> UpdatePetAsync(UpdatePetDto updatePetDto);
    Task<bool> DeletePetAsync(int petId);
}
