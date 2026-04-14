using PawNect.PetParent.Web.Models;

namespace PawNect.PetParent.Web.Services;

/// <summary>
/// Static mock slots: Available / Booked for A4.
/// </summary>
public static class MockSlotData
{
    private static readonly string[] Times = { "09:00", "09:30", "10:00", "10:30", "11:00", "14:00", "14:30", "15:00", "15:30", "16:00" };
    private static readonly Random Rnd = new(42); // Fixed seed for consistent "booked" slots

    public static List<SlotViewModel> GetSlotsForVet(int vetId, int daysAhead = 7)
    {
        var list = new List<SlotViewModel>();
        var baseDate = DateTime.Today;
        var dayNames = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        for (var d = 0; d < daysAhead; d++)
        {
            var date = baseDate.AddDays(d);
            var dayName = date.ToString("ddd");
            if (date.DayOfWeek == DayOfWeek.Sunday && vetId % 2 == 0) continue; // Some vets no Sunday
            foreach (var time in Times)
            {
                var isAvailable = Rnd.Next(100) > 25; // ~75% available
                list.Add(new SlotViewModel
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Time = time,
                    DisplayText = $"{dayName}, {date:dd MMM} · {time}",
                    IsAvailable = isAvailable
                });
            }
        }
        return list;
    }
}
