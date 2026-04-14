using PawNect.Application.DTOs.Appointment;

namespace PawNect.Application.Interfaces;

/// <summary>
/// Appointment Service interface
/// </summary>
public interface IAppointmentService
{
    Task<AppointmentDto?> GetByIdAsync(int id);
    Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto createAppointmentDto);
    Task<AppointmentDto?> UpdateAppointmentAsync(int id, UpdateAppointmentDto dto);
    Task<IEnumerable<AppointmentListDto>> GetByVetIdAsync(int vetId, DateTime? fromDate, bool upcomingOnly);
    Task<IEnumerable<AppointmentListDto>> GetByOwnerIdAsync(int ownerId);
}
