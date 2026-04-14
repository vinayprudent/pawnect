namespace PawNect.PetParent.Web.Models;

/// <summary>
/// A5 Verify details + A6 Confirmation
/// </summary>
/// <summary>Consult mode: InClinic (default) or Online. Used to set appointment type and copy.</summary>
public static class ConsultMode
{
    public const string InClinic = "InClinic";
    public const string Online = "Online";
}

public class BookingVerifyViewModel
{
    public int VetId { get; set; }
    public string VetName { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public string ClinicAddress { get; set; } = string.Empty;
    public string SlotDate { get; set; } = string.Empty;
    public string SlotTime { get; set; } = string.Empty;
    public string SlotDisplay { get; set; } = string.Empty;
    public int SelectedPetId { get; set; }
    public string? ReasonForVisit { get; set; }
    /// <summary>Optional symptoms / chief complaints (used in online consult flow).</summary>
    public string? Symptoms { get; set; }
    public string? PetParentName { get; set; }
    public string? PetParentEmail { get; set; }
    public string? PetParentPhone { get; set; }
    /// <summary>InClinic or Online – passed through booking flow to set appointment type.</summary>
    public string ConsultMode { get; set; } = "InClinic";
    public List<PetOptionViewModel> Pets { get; set; } = new();
}

public class PetOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
}

/// <summary>
/// A6 Confirmation + A7 Upcoming. Consult status: Booked → In Progress → Completed → Closed
/// </summary>
public class BookingConfirmViewModel
{
    public string BookingId { get; set; } = string.Empty;
    public int? VetId { get; set; }
    public string VetName { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public string ClinicAddress { get; set; } = string.Empty;
    public string SlotDate { get; set; } = string.Empty;
    public string SlotTime { get; set; } = string.Empty;
    public string PetName { get; set; } = string.Empty;
    /// <summary>Consult status: Booked, In Progress, Completed, Closed, or Cancelled</summary>
    public string Status { get; set; } = "Booked";
    public string? ReasonForVisit { get; set; }
    public int? PetId { get; set; }
    /// <summary>Backend database appointment ID (for reschedule/cancel API).</summary>
    public int? AppointmentId { get; set; }
    /// <summary>Parent (owner) user ID for vet-side listing.</summary>
    public int? ParentUserId { get; set; }
    public string? ParentName { get; set; }
    public string? ParentEmail { get; set; }
    public string? ParentPhone { get; set; }
}

/// <summary>B1 Vet dashboard: today's and upcoming appointments (from API).</summary>
public class VetDashboardViewModel
{
    public List<VetAppointmentItemViewModel> TodayAppointments { get; set; } = new();
    public List<VetAppointmentItemViewModel> UpcomingAppointments { get; set; } = new();
}

/// <summary>Single appointment item for vet dashboard (from API).</summary>
public class VetAppointmentItemViewModel
{
    public int Id { get; set; }
    public string PetName { get; set; } = string.Empty;
    public string? SlotDate { get; set; }
    public string? SlotTime { get; set; }
    public string? ParentName { get; set; }
    public string? ReasonForVisit { get; set; }
    public string Status { get; set; } = "Booked";
}

/// <summary>ViewModel for reschedule page: booking + available slots.</summary>
public class RescheduleViewModel
{
    public BookingConfirmViewModel Booking { get; set; } = null!;
    public List<SlotViewModel> Slots { get; set; } = new();
}
