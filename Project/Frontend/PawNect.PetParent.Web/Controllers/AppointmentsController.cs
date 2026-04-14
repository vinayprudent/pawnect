using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PawNect.PetParent.Web.Infrastructure;
using PawNect.PetParent.Web.Models;
using PawNect.PetParent.Web.Services;

namespace PawNect.PetParent.Web.Controllers;

/// <summary>
/// A7. Post-Booking – Upcoming Appointments. Cancel/Reschedule with database persistence.
/// </summary>
[RequireParentRole]
public class AppointmentsController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PawNect.PetParent.Web.Services.RatingApiService _ratingApiService;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AppointmentsController(IConfiguration configuration, IHttpClientFactory httpClientFactory, PawNect.PetParent.Web.Services.RatingApiService ratingApiService)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _ratingApiService = ratingApiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (!userId.HasValue)
        {
            TempData["ErrorMessage"] = "Please sign in to view your appointments.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index)) });
        }
        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
        var list = new List<BookingConfirmViewModel>();
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{baseUrl}/appointments/owner/{userId.Value}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponseViewModel<List<AppointmentListApiDto>>>(content, JsonOptions);
                if (apiResponse?.Data != null)
                {
                    list = apiResponse.Data.Select(dto => new BookingConfirmViewModel
                    {
                        AppointmentId = dto.Id,
                        BookingId = dto.Id.ToString(),
                        VetName = dto.ProviderName ?? "",
                        ClinicName = dto.Location ?? "",
                        SlotDate = dto.SlotDate ?? "",
                        SlotTime = dto.SlotTime ?? "",
                        PetName = dto.PetName ?? "",
                        Status = dto.Status ?? "Booked",
                        VetId = dto.VetId
                    }).ToList();
                }
            }
        }
        catch { /* show empty list */ }

        var ratedIds = new HashSet<string>();
        foreach (var b in list.Where(b => b.AppointmentId.HasValue))
        {
            if (await _ratingApiService.HasParentRatedBookingAsync(userId.Value, b.AppointmentId!.Value.ToString()))
                ratedIds.Add(b.AppointmentId.Value.ToString());
        }
        ViewBag.RatedBookingIds = ratedIds;
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int appointmentId)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
        try
        {
            var client = _httpClientFactory.CreateClient();
            var body = JsonSerializer.Serialize(new { isCancelled = true });
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{baseUrl}/appointments/{appointmentId}", content);
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Could not cancel appointment. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }
        catch
        {
            TempData["ErrorMessage"] = "Could not cancel appointment. Please check the connection and try again.";
            return RedirectToAction(nameof(Index));
        }
        TempData["SuccessMessage"] = "Appointment cancelled.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// GET: Show slot picker for rescheduling (same vet). Uses appointmentId from API.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Reschedule(int appointmentId)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{baseUrl}/appointments/{appointmentId}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponseViewModel<AppointmentApiDto>>(content, JsonOptions);
            var appointment = apiResponse?.Data;
            if (appointment == null || appointment.OwnerId != userId.Value)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }
            if (!appointment.VetId.HasValue)
            {
                TempData["ErrorMessage"] = "Cannot reschedule: vet information missing.";
                return RedirectToAction(nameof(Index));
            }
            var slotDate = appointment.StartTime.ToString("yyyy-MM-dd");
            var slotTime = appointment.StartTime.ToString("HH:mm");
            var booking = new BookingConfirmViewModel
            {
                AppointmentId = appointment.Id,
                BookingId = appointment.Id.ToString(),
                VetId = appointment.VetId,
                VetName = appointment.ProviderName ?? "",
                ClinicName = appointment.Location ?? "",
                SlotDate = slotDate,
                SlotTime = slotTime,
                PetName = "", // not required for reschedule view
                Status = appointment.Status ?? "Booked"
            };
            var slots = SlotService.GetSlotsForVet(appointment.VetId.Value);
            var model = new RescheduleViewModel { Booking = booking, Slots = slots };
            return View(model);
        }
        catch
        {
            TempData["ErrorMessage"] = "Could not load appointment. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// POST: Apply new slot and persist to database.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule(int appointmentId, string slotDate, string slotTime)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        if (!DateTime.TryParseExact($"{slotDate} {slotTime}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startTime))
            startTime = DateTime.Parse($"{slotDate} {slotTime}", CultureInfo.InvariantCulture);
        var endTime = startTime.AddMinutes(30);

        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
        var updateDto = new { startTime, endTime };
        try
        {
            var client = _httpClientFactory.CreateClient();
            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{baseUrl}/appointments/{appointmentId}", content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<ApiResponseViewModel<object>>(errorContent, JsonOptions);
                TempData["ErrorMessage"] = errorResponse?.Message ?? "Failed to reschedule. Please try again.";
                return RedirectToAction(nameof(Reschedule), new { appointmentId });
            }
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Unable to reschedule. Please check the connection and try again.";
            return RedirectToAction(nameof(Reschedule), new { appointmentId });
        }
        TempData["SuccessMessage"] = "Appointment rescheduled successfully.";
        return RedirectToAction(nameof(Index));
    }
}
