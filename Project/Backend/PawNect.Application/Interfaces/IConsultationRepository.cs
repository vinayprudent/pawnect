using PawNect.Domain.Entities;

namespace PawNect.Application.Interfaces;

public interface IConsultationRepository : IRepository<Consultation>
{
    Task<Consultation?> GetByAppointmentIdAsync(int appointmentId);
    Task<IEnumerable<Consultation>> GetByPetIdAsync(int petId, int excludeAppointmentId = 0);
}
