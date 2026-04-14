using PawNect.Application.DTOs.Consultation;

namespace PawNect.Application.Interfaces;

public interface IConsultationService
{
    Task<ConsultationDto?> GetByAppointmentIdAsync(int appointmentId);
    Task<IEnumerable<PreviousConsultSummaryDto>> GetPreviousConsultsByPetIdAsync(int petId, int excludeAppointmentId);
    Task<ConsultationDto> SaveAsync(SaveConsultationDto dto);
    Task<bool> SetPrescriptionUrlAsync(int consultationId, string prescriptionUrl);
}
