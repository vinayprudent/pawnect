using PawNect.PetParent.Web.Models;

namespace PawNect.PetParent.Web.Services;

/// <summary>
/// Per-vet availability calendar settings. If vet has not set any, defaults are used (365 days, 60 min batch, max 3 per slot).
/// </summary>
public static class VetAvailabilityStore
{
    private static readonly object Lock = new();
    private static readonly Dictionary<int, VetAvailabilitySettings> ByVetId = new();

    public static VetAvailabilitySettings GetSettings(int vetId)
    {
        lock (Lock)
        {
            if (ByVetId.TryGetValue(vetId, out var s))
            {
                s.VetId = vetId;
                return s;
            }
        }
        return new VetAvailabilitySettings
        {
            VetId = vetId,
            DaysAhead = VetAvailabilitySettings.DefaultDaysAhead,
            SlotDurationMinutes = VetAvailabilitySettings.DefaultSlotDurationMinutes,
            MaxAppointmentsPerSlot = VetAvailabilitySettings.DefaultMaxAppointmentsPerSlot,
            StartHour = VetAvailabilitySettings.DefaultStartHour,
            EndHour = VetAvailabilitySettings.DefaultEndHour
        };
    }

    public static void SetSettings(VetAvailabilitySettings settings)
    {
        if (settings == null) return;
        lock (Lock)
        {
            ByVetId[settings.VetId] = settings;
        }
    }
}
