namespace PawNect.PetParent.Web.Models;

/// <summary>
/// Vet-defined availability calendar: how far ahead to show slots, slot duration (batch), max patients per slot.
/// Defaults: 365 days, 60 min batch, 3 patients per slot.
/// </summary>
public class VetAvailabilitySettings
{
    public const int DefaultDaysAhead = 365;
    public const int DefaultSlotDurationMinutes = 60;
    public const int DefaultMaxAppointmentsPerSlot = 3;
    public const int DefaultStartHour = 9;
    public const int DefaultEndHour = 17;

    public int VetId { get; set; }
    /// <summary>Number of days ahead from today to show bookable slots. Default 365.</summary>
    public int DaysAhead { get; set; } = DefaultDaysAhead;
    /// <summary>Slot duration in minutes (e.g. 60 = 1-hour batch). Default 60.</summary>
    public int SlotDurationMinutes { get; set; } = DefaultSlotDurationMinutes;
    /// <summary>Max appointments in a single slot (batch). Default 3.</summary>
    public int MaxAppointmentsPerSlot { get; set; } = DefaultMaxAppointmentsPerSlot;
    /// <summary>Working day start hour (0-23). Default 9.</summary>
    public int StartHour { get; set; } = DefaultStartHour;
    /// <summary>Working day end hour (exclusive, 0-24). Default 17.</summary>
    public int EndHour { get; set; } = DefaultEndHour;
}
