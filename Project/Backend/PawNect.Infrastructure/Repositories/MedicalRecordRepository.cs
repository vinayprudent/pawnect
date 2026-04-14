using Microsoft.EntityFrameworkCore;
using PawNect.Application.Interfaces;
using PawNect.Domain.Entities;
using PawNect.Infrastructure.DbContext;

namespace PawNect.Infrastructure.Repositories;

/// <summary>
/// Medical Record Repository implementation
/// </summary>
public class MedicalRecordRepository : Repository<MedicalRecord>, IMedicalRecordRepository
{
    public MedicalRecordRepository(PawNectDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MedicalRecord>> GetRecordsByPetIdAsync(int petId)
    {
        return await _dbSet
            .Where(m => m.PetId == petId && !m.IsDeleted)
            .OrderByDescending(m => m.RecordDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<MedicalRecord>> GetRecordsByDateRangeAsync(int petId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(m => m.PetId == petId && !m.IsDeleted &&
                   m.RecordDate >= startDate && m.RecordDate <= endDate)
            .OrderByDescending(m => m.RecordDate)
            .ToListAsync();
    }
}
