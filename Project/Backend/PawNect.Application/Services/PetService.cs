using PawNect.Application.DTOs.Pet;
using PawNect.Application.Interfaces;
using PawNect.Domain.Entities;
using PawNect.Domain.Enums;
using PawNect.Domain.Rules;

namespace PawNect.Application.Services;

/// <summary>
/// Pet Service implementation
/// </summary>
public class PetService : IPetService
{
    private readonly IPetRepository _petRepository;

    public PetService(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    public async Task<PetDto?> GetPetByIdAsync(int petId)
    {
        var pet = await _petRepository.GetByIdAsync(petId);
        return pet == null ? null : MapToDto(pet);
    }

    public async Task<IEnumerable<PetDto>> GetAllPetsAsync()
    {
        var pets = await _petRepository.GetAllAsync();
        return pets.Select(MapToDto);
    }

    public async Task<IEnumerable<PetDto>> GetPetsByOwnerAsync(int ownerId)
    {
        var pets = await _petRepository.GetPetsByOwnerIdAsync(ownerId);
        return pets.Select(MapToDto);
    }

    public async Task<PetDto> CreatePetAsync(CreatePetDto createPetDto)
    {
        // Validate pet data
        var nameValidation = PetRules.Validations.ValidatePetName(createPetDto.Name);
        if (!nameValidation.IsValid)
            throw new ArgumentException(nameValidation.Message);

        var weightValidation = PetRules.Validations.ValidatePetWeight(createPetDto.WeightKg);
        if (!weightValidation.IsValid)
            throw new ArgumentException(weightValidation.Message);

        var dobValidation = PetRules.Validations.ValidateDateOfBirth(createPetDto.DateOfBirth);
        if (!dobValidation.IsValid)
            throw new ArgumentException(dobValidation.Message);

        var pet = new Pet
        {
            Name = createPetDto.Name,
            Breed = createPetDto.Breed,
            Species = (PetSpecies)createPetDto.Species,
            DateOfBirth = createPetDto.DateOfBirth,
            WeightKg = createPetDto.WeightKg,
            Color = createPetDto.Color,
            MicrochipId = createPetDto.MicrochipId,
            ProfileImageUrl = createPetDto.ProfileImageUrl,
            Description = createPetDto.Description,
            OwnerId = createPetDto.OwnerId,
            Status = PetStatus.Active
        };

        var createdPet = await _petRepository.AddAsync(pet);
        await _petRepository.SaveChangesAsync();

        return MapToDto(createdPet);
    }

    public async Task<PetDto> UpdatePetAsync(UpdatePetDto updatePetDto)
    {
        var pet = await _petRepository.GetByIdAsync(updatePetDto.Id);
        if (pet == null)
            throw new KeyNotFoundException($"Pet with ID {updatePetDto.Id} not found.");

        // Validate updates
        var nameValidation = PetRules.Validations.ValidatePetName(updatePetDto.Name);
        if (!nameValidation.IsValid)
            throw new ArgumentException(nameValidation.Message);

        pet.Name = updatePetDto.Name;
        pet.Breed = updatePetDto.Breed;
        pet.Species = (PetSpecies)updatePetDto.Species;
        pet.DateOfBirth = updatePetDto.DateOfBirth;
        pet.WeightKg = updatePetDto.WeightKg;
        pet.Color = updatePetDto.Color;
        pet.MicrochipId = updatePetDto.MicrochipId;
        pet.Status = (PetStatus)updatePetDto.Status;
        pet.ProfileImageUrl = updatePetDto.ProfileImageUrl;
        pet.Description = updatePetDto.Description;
        pet.UpdatedAt = DateTime.UtcNow;

        var updatedPet = await _petRepository.UpdateAsync(pet);
        await _petRepository.SaveChangesAsync();

        return MapToDto(updatedPet);
    }

    public async Task<bool> DeletePetAsync(int petId)
    {
        var success = await _petRepository.DeleteAsync(petId);
        if (success)
            await _petRepository.SaveChangesAsync();

        return success;
    }

    private static PetDto MapToDto(Pet pet)
    {
        return new PetDto
        {
            Id = pet.Id,
            Name = pet.Name,
            Breed = pet.Breed,
            Species = (int)pet.Species,
            DateOfBirth = pet.DateOfBirth,
            WeightKg = pet.WeightKg,
            Color = pet.Color,
            MicrochipId = pet.MicrochipId,
            Status = (int)pet.Status,
            ProfileImageUrl = pet.ProfileImageUrl,
            Description = pet.Description,
            OwnerId = pet.OwnerId,
            CreatedAt = pet.CreatedAt,
            UpdatedAt = pet.UpdatedAt
        };
    }
}
