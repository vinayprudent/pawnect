using Microsoft.EntityFrameworkCore;
using PawNect.Application.Interfaces;
using PawNect.Domain.Entities;
using PawNect.Infrastructure.DbContext;

namespace PawNect.Infrastructure.Repositories;

/// <summary>
/// Pet Repository implementation
/// </summary>
public class PetRepository : Repository<Pet>, IPetRepository
{
    public PetRepository(PawNectDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Pet>> GetPetsByOwnerIdAsync(int ownerId)
    {
        return await _dbSet
            .Where(p => p.OwnerId == ownerId && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<Pet?> GetPetWithMedicalRecordsAsync(int petId)
    {
        return await _dbSet
            .Include(p => p.MedicalRecords)
            .FirstOrDefaultAsync(p => p.Id == petId && !p.IsDeleted);
    }

    public async Task<Pet?> GetPetWithAppointmentsAsync(int petId)
    {
        return await _dbSet
            .Include(p => p.Appointments)
            .FirstOrDefaultAsync(p => p.Id == petId && !p.IsDeleted);
    }

    public async Task<IEnumerable<Pet>> SearchPetsAsync(string searchTerm)
    {
        return await _dbSet
            .Where(p => !p.IsDeleted && 
                   (p.Name.Contains(searchTerm) || 
                    p.Breed != null && p.Breed.Contains(searchTerm)))
            .ToListAsync();
    }
}
