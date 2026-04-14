using PawNect.Domain.Entities;

namespace PawNect.Application.Interfaces;

/// <summary>
/// Medical Record Repository interface
/// </summary>
public interface IMedicalRecordRepository : IRepository<MedicalRecord>
{
    Task<IEnumerable<MedicalRecord>> GetRecordsByPetIdAsync(int petId);
    Task<IEnumerable<MedicalRecord>> GetRecordsByDateRangeAsync(int petId, DateTime startDate, DateTime endDate);
}
