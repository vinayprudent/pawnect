using PawNect.Application.DTOs.Consultation;
using PawNect.Application.Interfaces;
using PawNect.Domain.Entities;

namespace PawNect.Application.Services;

public class ConsultationService : IConsultationService
{
    private readonly IConsultationRepository _consultationRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public ConsultationService(
        IConsultationRepository consultationRepository,
        IAppointmentRepository appointmentRepository)
    {
        _consultationRepository = consultationRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<ConsultationDto?> GetByAppointmentIdAsync(int appointmentId)
    {
        var apt = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);
        if (apt == null)
            return null;

        var consultation = await _consultationRepository.GetByAppointmentIdAsync(appointmentId);
        if (consultation != null)
            return MapToDto(consultation, consultation.Appointment ?? apt);

        // No consultation yet - build from appointment
        return new ConsultationDto
        {
            Id = 0,
            AppointmentId = appointmentId,
            VetId = apt.VetId ?? 0,
            PetId = apt.PetId,
            OwnerId = apt.OwnerId ?? 0,
            Status = apt.Status,
            ReasonForVisit = apt.Notes,
            PetName = apt.Pet?.Name,
            PetSpecies = apt.Pet?.Species.ToString(),
            PetBreed = apt.Pet?.Breed,
            PetWeightKg = apt.Pet?.WeightKg,
            ParentName = apt.Owner != null ? $"{apt.Owner.FirstName} {apt.Owner.LastName}".Trim() : null,
            ParentEmail = apt.Owner?.Email,
            ParentPhone = apt.Owner?.PhoneNumber,
            VetName = apt.ProviderName,
            ClinicAddress = apt.Location,
            SlotStart = apt.StartTime
        };
    }

    public async Task<IEnumerable<PreviousConsultSummaryDto>> GetPreviousConsultsByPetIdAsync(int petId, int excludeAppointmentId)
    {
        var list = await _consultationRepository.GetByPetIdAsync(petId, excludeAppointmentId);
        return list.Select(c => new PreviousConsultSummaryDto
        {
            Date = c.Appointment?.StartTime.ToString("yyyy-MM-dd") ?? c.CreatedAt.ToString("yyyy-MM-dd"),
            VetName = c.Appointment?.ProviderName ?? "—",
            Summary = string.IsNullOrEmpty(c.ProvisionalDiagnosis) ? (c.Notes?.Length > 100 ? c.Notes[..100] + "…" : c.Notes) ?? "—" : (c.ProvisionalDiagnosis.Length > 100 ? c.ProvisionalDiagnosis[..100] + "…" : c.ProvisionalDiagnosis)
        });
    }

    public async Task<ConsultationDto> SaveAsync(SaveConsultationDto dto)
    {
        var existing = await _consultationRepository.GetByAppointmentIdAsync(dto.AppointmentId);
        Consultation consultation;
        if (existing != null)
        {
            existing.Notes = dto.Notes;
            existing.ProvisionalDiagnosis = dto.ProvisionalDiagnosis;
            existing.PrescriptionUrl = dto.PrescriptionUrl;
            existing.VitalsJson = dto.VitalsJson;
            existing.ConsultationComplete = dto.ConsultationComplete;
            existing.DiagnosticsRecommended = dto.DiagnosticsRecommended;
            if (!string.IsNullOrWhiteSpace(dto.Status))
                existing.Status = dto.Status;
            existing.UpdatedAt = DateTime.UtcNow;
            await _consultationRepository.UpdateAsync(existing);
            consultation = existing;
        }
        else
        {
            consultation = new Consultation
            {
                AppointmentId = dto.AppointmentId,
                VetId = dto.VetId,
                PetId = dto.PetId,
                OwnerId = dto.OwnerId,
                Status = dto.Status ?? "Booked",
                Notes = dto.Notes,
                ProvisionalDiagnosis = dto.ProvisionalDiagnosis,
                PrescriptionUrl = dto.PrescriptionUrl,
                VitalsJson = dto.VitalsJson,
                ConsultationComplete = dto.ConsultationComplete,
                DiagnosticsRecommended = dto.DiagnosticsRecommended
            };
            await _consultationRepository.AddAsync(consultation);
        }
        await _consultationRepository.SaveChangesAsync();

        // Sync appointment status when vet updates consult status
        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            var apt = await _appointmentRepository.GetByIdAsync(dto.AppointmentId);
            if (apt != null && apt.Status != dto.Status)
            {
                apt.Status = dto.Status;
                apt.UpdatedAt = DateTime.UtcNow;
                await _appointmentRepository.UpdateAsync(apt);
                await _appointmentRepository.SaveChangesAsync();
            }
        }

        var aptWithDetails = await _appointmentRepository.GetByIdWithDetailsAsync(dto.AppointmentId);
        return MapToDto(consultation, aptWithDetails!);
    }

    public async Task<bool> SetPrescriptionUrlAsync(int consultationId, string prescriptionUrl)
    {
        var c = await _consultationRepository.GetByIdAsync(consultationId);
        if (c == null) return false;
        c.PrescriptionUrl = prescriptionUrl;
        c.UpdatedAt = DateTime.UtcNow;
        await _consultationRepository.UpdateAsync(c);
        await _consultationRepository.SaveChangesAsync();
        return true;
    }

    private static ConsultationDto MapToDto(Consultation c, Appointment? apt)
    {
        return new ConsultationDto
        {
            Id = c.Id,
            AppointmentId = c.AppointmentId,
            VetId = c.VetId,
            PetId = c.PetId,
            OwnerId = c.OwnerId,
            Status = c.Status,
            Notes = c.Notes,
            ProvisionalDiagnosis = c.ProvisionalDiagnosis,
            PrescriptionUrl = c.PrescriptionUrl,
            VitalsJson = c.VitalsJson,
            ConsultationComplete = c.ConsultationComplete,
            DiagnosticsRecommended = c.DiagnosticsRecommended,
            PetName = apt?.Pet?.Name ?? c.Pet?.Name,
            PetSpecies = apt?.Pet?.Species.ToString() ?? c.Pet?.Species.ToString(),
            PetBreed = apt?.Pet?.Breed ?? c.Pet?.Breed,
            PetWeightKg = apt?.Pet?.WeightKg ?? c.Pet?.WeightKg,
            ParentName = apt?.Owner != null ? $"{apt.Owner.FirstName} {apt.Owner.LastName}".Trim() : null,
            ParentEmail = apt?.Owner?.Email,
            ParentPhone = apt?.Owner?.PhoneNumber,
            ReasonForVisit = apt?.Notes,
            VetName = apt?.ProviderName,
            ClinicAddress = apt?.Location,
            SlotStart = apt?.StartTime
        };
    }
}
