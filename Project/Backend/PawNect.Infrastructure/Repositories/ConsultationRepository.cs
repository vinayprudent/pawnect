using Microsoft.EntityFrameworkCore;
using PawNect.Application.Interfaces;
using PawNect.Domain.Entities;
using PawNect.Infrastructure.DbContext;

namespace PawNect.Infrastructure.Repositories;

public class ConsultationRepository : Repository<Consultation>, IConsultationRepository
{
    public ConsultationRepository(PawNectDbContext context) : base(context)
    {
    }

    public async Task<Consultation?> GetByAppointmentIdAsync(int appointmentId)
    {
        return await _dbSet
            .Include(c => c.Appointment).ThenInclude(a => a!.Pet)
            .Include(c => c.Appointment).ThenInclude(a => a!.Owner)
            .FirstOrDefaultAsync(c => c.AppointmentId == appointmentId && !c.IsDeleted);
    }

    public async Task<IEnumerable<Consultation>> GetByPetIdAsync(int petId, int excludeAppointmentId = 0)
    {
        var query = _dbSet
            .Include(c => c.Appointment)
            .Where(c => c.PetId == petId && !c.IsDeleted && c.ConsultationComplete);
        if (excludeAppointmentId != 0)
            query = query.Where(c => c.AppointmentId != excludeAppointmentId);
        return await query.OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt).Take(20).ToListAsync();
    }
}
