using PawNect.PetParent.Web.Models;

namespace PawNect.PetParent.Web.Services;

/// <summary>
/// Generates bookable slots for a vet based on their availability settings.
/// Defaults: 365 days ahead, 60-min batch, max 3 patients per slot. Vet can override in Availability.
/// </summary>
public static class SlotService
{
    public static List<SlotViewModel> GetSlotsForVet(int vetId)
    {
        var settings = VetAvailabilityStore.GetSettings(vetId);
        var list = new List<SlotViewModel>();
        var baseDate = DateTime.Today;

        for (var d = 0; d < settings.DaysAhead; d++)
        {
            var date = baseDate.AddDays(d);
            var dateStr = date.ToString("yyyy-MM-dd");
            var dayName = date.ToString("ddd");

            var slotMinutes = settings.StartHour * 60;
            var endMinutes = settings.EndHour * 60;
            if (endMinutes <= slotMinutes && settings.EndHour < 24) endMinutes += 24 * 60;

            while (slotMinutes + settings.SlotDurationMinutes <= endMinutes)
            {
                var hour = (slotMinutes / 60) % 24;
                var min = slotMinutes % 60;
                var timeStr = $"{hour:D2}:{min:D2}";
                var bookedCount = BookingStore.GetBookedCountForSlot(vetId, dateStr, timeStr);
                var isAvailable = bookedCount < settings.MaxAppointmentsPerSlot;

                list.Add(new SlotViewModel
                {
                    Date = dateStr,
                    Time = timeStr,
                    DisplayText = $"{dayName}, {date:dd MMM} · {timeStr}",
                    IsAvailable = isAvailable
                });

                slotMinutes += settings.SlotDurationMinutes;
            }
        }

        return list;
    }
}
