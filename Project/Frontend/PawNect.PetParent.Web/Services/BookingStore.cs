using PawNect.PetParent.Web.Models;

namespace PawNect.PetParent.Web.Services;

/// <summary>
/// In-memory store for demo bookings (A6/A7). Keyed by user ID (parent) and by vet ID for vet dashboard.
/// </summary>
public static class BookingStore
{
    private static readonly object Lock = new();
    private static readonly Dictionary<int, List<BookingConfirmViewModel>> ByUser = new();
    private static readonly Dictionary<string, BookingConfirmViewModel> ByBookingId = new();
    private static int _nextId = 1000;

    public static string Add(int userId, BookingConfirmViewModel booking)
    {
        lock (Lock)
        {
            booking.BookingId = $"BK-{_nextId++}";
            booking.Status = "Booked";
            booking.ParentUserId = userId;
            if (!ByUser.ContainsKey(userId))
                ByUser[userId] = new List<BookingConfirmViewModel>();
            ByUser[userId].Add(booking);
            ByBookingId[booking.BookingId] = booking;
            return booking.BookingId;
        }
    }

    public static IReadOnlyList<BookingConfirmViewModel> GetByUser(int userId)
    {
        lock (Lock)
        {
            if (!ByUser.TryGetValue(userId, out var list))
                return Array.Empty<BookingConfirmViewModel>();
            return list.Where(b => b.Status != "Cancelled").OrderByDescending(b => b.SlotDate + b.SlotTime).ToList();
        }
    }

    public static BookingConfirmViewModel? GetByBookingId(int userId, string bookingId)
    {
        lock (Lock)
        {
            if (!ByUser.TryGetValue(userId, out var list))
                return null;
            return list.FirstOrDefault(b => b.BookingId == bookingId);
        }
    }

    /// <summary>
    /// Get booking by ID (for vet consult screen; no user filter).
    /// </summary>
    public static BookingConfirmViewModel? GetByBookingId(string bookingId)
    {
        lock (Lock)
        {
            return ByBookingId.TryGetValue(bookingId, out var b) ? b : null;
        }
    }

    /// <summary>
    /// Get today's and upcoming appointments for a vet. Excludes cancelled.
    /// </summary>
    public static (IReadOnlyList<BookingConfirmViewModel> Today, IReadOnlyList<BookingConfirmViewModel> Upcoming) GetByVetId(int vetId, string todayDate)
    {
        lock (Lock)
        {
            var all = ByUser.Values
                .SelectMany(list => list)
                .Where(b => b.VetId == vetId && b.Status != "Cancelled")
                .OrderBy(b => b.SlotDate).ThenBy(b => b.SlotTime)
                .ToList();
            var today = all.Where(b => b.SlotDate == todayDate).ToList();
            var upcoming = all.Where(b => string.Compare(b.SlotDate, todayDate, StringComparison.Ordinal) > 0).ToList();
            return (today, upcoming);
        }
    }

    /// <summary>
    /// All appointments for vet (for Consultations list). Excludes cancelled.
    /// </summary>
    public static IReadOnlyList<BookingConfirmViewModel> GetAllByVetId(int vetId)
    {
        lock (Lock)
        {
            return ByUser.Values
                .SelectMany(list => list)
                .Where(b => b.VetId == vetId && b.Status != "Cancelled")
                .OrderByDescending(b => b.SlotDate).ThenByDescending(b => b.SlotTime)
                .ToList();
        }
    }

    /// <summary>
    /// UI only - mark as cancelled for demo.
    /// </summary>
    public static void Cancel(int userId, string bookingId)
    {
        lock (Lock)
        {
            if (!ByUser.TryGetValue(userId, out var list))
                return;
            var b = list.FirstOrDefault(x => x.BookingId == bookingId);
            if (b != null)
                b.Status = "Cancelled";
        }
    }

    /// <summary>
    /// Update slot (date/time) for a booking (after reschedule).
    /// </summary>
    public static void UpdateSlot(int userId, string bookingId, string slotDate, string slotTime)
    {
        lock (Lock)
        {
            if (!ByUser.TryGetValue(userId, out var list))
                return;
            var b = list.FirstOrDefault(x => x.BookingId == bookingId);
            if (b != null)
            {
                b.SlotDate = slotDate;
                b.SlotTime = slotTime;
                if (ByBookingId.TryGetValue(bookingId, out var same))
                {
                    same.SlotDate = slotDate;
                    same.SlotTime = slotTime;
                }
            }
        }
    }

    /// <summary>
    /// Update consult status (vet marks In Progress, Completed, etc.).
    /// </summary>
    public static void UpdateStatus(string bookingId, string status)
    {
        lock (Lock)
        {
            if (ByBookingId.TryGetValue(bookingId, out var b))
                b.Status = status;
        }
    }

    /// <summary>
    /// Count of non-cancelled bookings for a vet at a given slot (date + time). Used for capacity (max per slot).
    /// </summary>
    public static int GetBookedCountForSlot(int vetId, string slotDate, string slotTime)
    {
        lock (Lock)
        {
            return ByUser.Values
                .SelectMany(list => list)
                .Count(b => b.VetId == vetId && b.Status != "Cancelled" && b.SlotDate == slotDate && b.SlotTime == slotTime);
        }
    }
}
