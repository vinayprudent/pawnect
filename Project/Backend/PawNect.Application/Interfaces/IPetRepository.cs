using PawNect.Domain.Entities;

namespace PawNect.Application.Interfaces;

/// <summary>
/// Pet Repository interface
/// </summary>
public interface IPetRepository : IRepository<Pet>
{
    Task<IEnumerable<Pet>> GetPetsByOwnerIdAsync(int ownerId);
    Task<Pet?> GetPetWithMedicalRecordsAsync(int petId);
    Task<Pet?> GetPetWithAppointmentsAsync(int petId);
    Task<IEnumerable<Pet>> SearchPetsAsync(string searchTerm);
}
