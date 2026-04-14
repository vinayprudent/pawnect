using PawNect.Application.DTOs.Appointment;
using PawNect.Application.Interfaces;
using PawNect.Domain.Entities;

namespace PawNect.Application.Services;

/// <summary>
/// Appointment Service implementation
/// </summary>
public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentService(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<AppointmentDto?> GetByIdAsync(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        return appointment == null ? null : MapToDto(appointment);
    }

    public async Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto dto)
    {
        var appointment = new Appointment
        {
            PetId = dto.PetId,
            Title = dto.Title,
            AppointmentType = dto.AppointmentType,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Location = dto.Location,
            ProviderName = dto.ProviderName,
            ProviderContact = dto.ProviderContact,
            Notes = dto.Notes,
            IsCompleted = false,
            IsCancelled = false,
            Status = "Booked",
            VetId = dto.VetId,
            OwnerId = dto.OwnerId
        };

        var created = await _appointmentRepository.AddAsync(appointment);
        await _appointmentRepository.SaveChangesAsync();

        return MapToDto(created);
    }

    public async Task<AppointmentDto?> UpdateAppointmentAsync(int id, UpdateAppointmentDto dto)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            return null;

        if (dto.StartTime.HasValue)
            appointment.StartTime = dto.StartTime.Value;
        if (dto.EndTime.HasValue)
            appointment.EndTime = dto.EndTime.Value;
        if (dto.IsCancelled.HasValue)
        {
            appointment.IsCancelled = dto.IsCancelled.Value;
            if (dto.IsCancelled.Value) appointment.Status = "Cancelled";
        }
        if (!string.IsNullOrWhiteSpace(dto.Status))
            appointment.Status = dto.Status;

        appointment.UpdatedAt = DateTime.UtcNow;
        await _appointmentRepository.UpdateAsync(appointment);
        await _appointmentRepository.SaveChangesAsync();

        return MapToDto(appointment);
    }

    public async Task<IEnumerable<AppointmentListDto>> GetByVetIdAsync(int vetId, DateTime? fromDate = null, bool upcomingOnly = false)
    {
        var list = await _appointmentRepository.GetAppointmentsByVetIdAsync(vetId, fromDate, upcomingOnly);
        return list.Select(MapToListDto);
    }

    public async Task<IEnumerable<AppointmentListDto>> GetByOwnerIdAsync(int ownerId)
    {
        var list = await _appointmentRepository.GetAppointmentsByOwnerIdAsync(ownerId);
        return list.Select(MapToListDto);
    }

    private static AppointmentListDto MapToListDto(Appointment a)
    {
        return new AppointmentListDto
        {
            Id = a.Id,
            PetId = a.PetId,
            PetName = a.Pet?.Name ?? "",
            SlotDate = a.StartTime.ToString("yyyy-MM-dd"),
            SlotTime = a.StartTime.ToString("HH:mm"),
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Notes = a.Notes,
            Status = a.Status,
            ProviderName = a.ProviderName,
            Location = a.Location,
            VetId = a.VetId,
            OwnerId = a.OwnerId,
            ParentName = a.Owner != null ? $"{a.Owner.FirstName} {a.Owner.LastName}".Trim() : null,
            ParentEmail = a.Owner?.Email,
            ParentPhone = a.Owner?.PhoneNumber
        };
    }

    private static AppointmentDto MapToDto(Appointment a)
    {
        return new AppointmentDto
        {
            Id = a.Id,
            PetId = a.PetId,
            Title = a.Title,
            AppointmentType = a.AppointmentType,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Location = a.Location,
            ProviderName = a.ProviderName,
            ProviderContact = a.ProviderContact,
            Notes = a.Notes,
            IsCompleted = a.IsCompleted,
            IsCancelled = a.IsCancelled,
            Status = a.Status,
            VetId = a.VetId,
            OwnerId = a.OwnerId,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        };
    }
}
