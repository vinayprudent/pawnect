using Microsoft.EntityFrameworkCore;
using PawNect.Application.Interfaces;
using PawNect.Domain.Entities;
using PawNect.Infrastructure.DbContext;

namespace PawNect.Infrastructure.Repositories;

/// <summary>
/// Appointment Repository implementation
/// </summary>
public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(PawNectDbContext context) : base(context)
    {
    }

    public async Task<Appointment?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(a => a.Pet)
            .Include(a => a.Owner)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByPetIdAsync(int petId)
    {
        return await _dbSet
            .Where(a => a.PetId == petId && !a.IsDeleted)
            .OrderByDescending(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(int petId)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(a => a.PetId == petId && !a.IsDeleted && a.StartTime > now && !a.IsCancelled)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(int petId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(a => a.PetId == petId && !a.IsDeleted &&
                   a.StartTime >= startDate && a.StartTime <= endDate)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByVetIdAsync(int vetId, DateTime? fromDate = null, bool upcomingOnly = false)
    {
        var query = _dbSet
            .Include(a => a.Pet)
            .Include(a => a.Owner)
            .Where(a => a.VetId == vetId && !a.IsDeleted && !a.IsCancelled);
        if (fromDate.HasValue)
            query = query.Where(a => a.StartTime.Date >= fromDate.Value.Date);
        if (upcomingOnly)
            query = query.Where(a => a.StartTime >= DateTime.UtcNow);
        return await query.OrderBy(a => a.StartTime).ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByOwnerIdAsync(int ownerId)
    {
        return await _dbSet
            .Include(a => a.Pet)
            .Include(a => a.Vet)
            .Where(a => a.OwnerId == ownerId && !a.IsDeleted && !a.IsCancelled)
            .OrderByDescending(a => a.StartTime)
            .ToListAsync();
    }
}
