using PawNect.Domain.Entities;

namespace PawNect.Application.Interfaces;

/// <summary>
/// Appointment Repository interface
/// </summary>
public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<Appointment?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Appointment>> GetAppointmentsByPetIdAsync(int petId);
    Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(int petId);
    Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(int petId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Appointment>> GetAppointmentsByVetIdAsync(int vetId, DateTime? fromDate = null, bool upcomingOnly = false);
    Task<IEnumerable<Appointment>> GetAppointmentsByOwnerIdAsync(int ownerId);
}
