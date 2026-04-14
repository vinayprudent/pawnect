using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PawNect.PetParent.Web.Infrastructure;
using PawNect.PetParent.Web.Models;
using PawNect.PetParent.Web.Services;

namespace PawNect.PetParent.Web.Controllers;

/// <summary>
/// Vet Doctor Flow – Pet Parent view: consultation details, diagnostics, report.
/// Status: Consult Booked → In Progress → Completed → Closed; Diagnostics Recommended → Sample Collected → Processing → Report Available → Reviewed.
/// </summary>
[RequireParentRole]
public class ConsultationsController : Controller
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RatingApiService _ratingApiService;

    public ConsultationsController(IConfiguration configuration, IHttpClientFactory httpClientFactory, RatingApiService ratingApiService)
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
            TempData["ErrorMessage"] = "Please sign in to view your consultations.";
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
                    list = apiResponse.Data.Select(dto => new BookingConfirmViewModel
                    {
                        AppointmentId = dto.Id,
                        BookingId = dto.Id.ToString(),
                        VetName = dto.ProviderName ?? "",
                        ClinicName = dto.Location ?? "",
                        SlotDate = dto.SlotDate ?? "",
                        SlotTime = dto.SlotTime ?? "",
                        PetName = dto.PetName ?? "",
                        Status = dto.Status ?? "Booked"
                    }).ToList();
            }
        }
        catch { /* show empty list */ }
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (!userId.HasValue)
        {
            TempData["ErrorMessage"] = "Please sign in to view consultation details.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Details), new { id }) });
        }
        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
        var client = _httpClientFactory.CreateClient();
        ConsultationDetailsViewModel? details = null;

        // Load consultation by appointment id
        var consultResponse = await client.GetAsync($"{baseUrl}/consultations/appointment/{id}");
        if (consultResponse.IsSuccessStatusCode)
        {
            var consultContent = await consultResponse.Content.ReadAsStringAsync();
            var consultWrap = JsonSerializer.Deserialize<ApiResponseViewModel<ConsultationApiDto>>(consultContent, JsonOptions);
            var consultation = consultWrap?.Data;
            if (consultation != null)
            {
                details = MapConsultationToDetails(consultation, id.ToString());
                if (!string.IsNullOrEmpty(consultation.PrescriptionUrl))
                    details.PrescriptionUrl = $"{baseUrl}/consultations/prescription-file/{consultation.Id}";
                if (consultation.Id > 0)
                {
                    var orderResponse = await client.GetAsync($"{baseUrl}/diagnostics/orders/consultation/{consultation.Id}");
                    if (orderResponse.IsSuccessStatusCode)
                    {
                        var orderContent = await orderResponse.Content.ReadAsStringAsync();
                        var orderWrap = JsonSerializer.Deserialize<ApiResponseViewModel<DiagnosticOrderApiDto>>(orderContent, JsonOptions);
                        if (orderWrap?.Data != null)
                            details.DiagnosticOrder = MapDiagnosticOrder(orderWrap.Data);
                    }
                    var reportResponse = await client.GetAsync($"{baseUrl}/diagnostics/reports/consultation/{consultation.Id}");
                    if (reportResponse.IsSuccessStatusCode)
                    {
                        var reportContent = await reportResponse.Content.ReadAsStringAsync();
                        var reportWrap = JsonSerializer.Deserialize<ApiResponseViewModel<DiagnosticReportApiDto>>(reportContent, JsonOptions);
                        if (reportWrap?.Data != null)
                            details.Report = MapReport(reportWrap.Data, baseUrl);
                    }
                }
            }
        }

        if (details == null)
        {
            // No consultation yet: show minimal view from appointment
            var appResponse = await client.GetAsync($"{baseUrl}/appointments/{id}");
            if (appResponse.IsSuccessStatusCode)
            {
                var appContent = await appResponse.Content.ReadAsStringAsync();
                var appWrap = JsonSerializer.Deserialize<ApiResponseViewModel<AppointmentApiDto>>(appContent, JsonOptions);
                var appointment = appWrap?.Data;
                if (appointment != null && appointment.OwnerId == userId.Value)
                {
                    details = new ConsultationDetailsViewModel
                    {
                        BookingId = id.ToString(),
                        ConsultationId = 0,
                        ConsultStatus = "Booked",
                        PetName = "",
                        SlotDate = appointment.StartTime.ToString("yyyy-MM-dd"),
                        SlotTime = appointment.StartTime.ToString("HH:mm"),
                        VetName = appointment.ProviderName ?? "",
                        ClinicName = appointment.Location ?? "",
                        ParentName = SessionHelper.GetUserName(HttpContext.Session),
                        ParentEmail = SessionHelper.GetUserEmail(HttpContext.Session),
                        VetId = appointment.VetId,
                        OwnerId = appointment.OwnerId
                    };
                }
            }
        }

        if (details == null)
        {
            TempData["ErrorMessage"] = "Consultation not found.";
            return RedirectToAction(nameof(Index));
        }

        details.ParentName ??= SessionHelper.GetUserName(HttpContext.Session);
        details.ParentEmail ??= SessionHelper.GetUserEmail(HttpContext.Session);
        ViewBag.HasRatedVet = details.VetId.HasValue && await _ratingApiService.HasParentRatedBookingAsync(userId.Value, id.ToString());
        return View(details);
    }

    private static ConsultationDetailsViewModel MapConsultationToDetails(ConsultationApiDto c, string bookingId)
    {
        var slotDate = c.SlotStart.HasValue ? c.SlotStart.Value.ToString("yyyy-MM-dd") : "";
        var slotTime = c.SlotStart.HasValue ? c.SlotStart.Value.ToString("HH:mm") : "";
        var vitals = new List<VitalViewModel>();
        if (!string.IsNullOrWhiteSpace(c.VitalsJson))
        {
            try
            {
                var entries = JsonSerializer.Deserialize<List<VitalEntryDto>>(c.VitalsJson);
                if (entries != null)
                    vitals = entries.Select(v => new VitalViewModel { Name = v.Name, Value = v.Value, Unit = v.Unit, RecordedAt = v.RecordedAt }).ToList();
            }
            catch { /* ignore */ }
        }
        string? prescriptionUrl = null;
        if (!string.IsNullOrWhiteSpace(c.PrescriptionUrl))
            prescriptionUrl = c.PrescriptionUrl.StartsWith("http") ? c.PrescriptionUrl : null; // full URL if API returns it
        return new ConsultationDetailsViewModel
        {
            BookingId = bookingId,
            ConsultationId = c.Id,
            ConsultStatus = c.Status ?? "Booked",
            PetName = c.PetName ?? "",
            PetSpecies = c.PetSpecies ?? "",
            PetBreed = c.PetBreed,
            PetWeightKg = c.PetWeightKg,
            ParentName = c.ParentName,
            ParentEmail = c.ParentEmail,
            VetId = c.VetId,
            PetId = c.PetId,
            OwnerId = c.OwnerId,
            VetName = c.VetName ?? "",
            ClinicName = c.ClinicName ?? "",
            ClinicAddress = c.ClinicAddress ?? "",
            SlotDate = slotDate,
            SlotTime = slotTime,
            ReasonForVisit = c.ReasonForVisit ?? "",
            PreviousConsults = new List<PreviousConsultSummaryViewModel>(),
            Vitals = vitals,
            Notes = c.Notes,
            ProvisionalDiagnosis = c.ProvisionalDiagnosis,
            PrescriptionUrl = prescriptionUrl,
            PrescriptionFileName = !string.IsNullOrWhiteSpace(c.PrescriptionUrl) ? "Prescription" : null,
            ConsultationComplete = c.ConsultationComplete,
            DiagnosticsRecommended = c.DiagnosticsRecommended
        };
    }

    private static DiagnosticOrderViewModel MapDiagnosticOrder(DiagnosticOrderApiDto d)
    {
        return new DiagnosticOrderViewModel
        {
            OrderId = d.Id.ToString(),
            Status = d.Status ?? "Ordered",
            TestsOrdered = d.TestsOrdered ?? new List<string>(),
            CollectionType = "In-Clinic",
            TotalPrice = d.TotalPrice,
            AssignedLab = d.AssignedLabName,
            OrderedAt = d.OrderedAt?.ToString("g"),
            SampleCollectedAt = d.SampleCollectedAt?.ToString("g"),
            ReportUploadedAt = d.ReportUploadedAt?.ToString("g")
        };
    }

    private static LabReportViewModel MapReport(DiagnosticReportApiDto r, string apiBaseUrl)
    {
        var baseUrl = apiBaseUrl?.TrimEnd('/') ?? "";
        return new LabReportViewModel
        {
            ReportId = r.Id.ToString(),
            ReportUrl = string.IsNullOrEmpty(baseUrl) ? null : $"{baseUrl}/diagnostics/report-file/{r.Id}",
            ReportFileName = r.ReportFileName,
            VetAdvice = r.VetAdvice,
            NextSteps = r.NextSteps,
            ReviewedAt = r.ReviewedAt?.ToString("g")
        };
    }

    /// <summary>Parent rates vet after a visit (1-5 stars, optional comment). bookingId is appointmentId when using API.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RateVet(string bookingId, int vetId, int rating, string? comment)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");
        if (string.IsNullOrEmpty(bookingId) || !int.TryParse(bookingId, out var appointmentId))
        {
            TempData["ErrorMessage"] = "Invalid booking.";
            return RedirectToAction(nameof(Index));
        }
        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{baseUrl}/appointments/{appointmentId}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction(nameof(Index));
            }
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponseViewModel<AppointmentApiDto>>(content, JsonOptions);
            var appointment = apiResponse?.Data;
            if (appointment == null || appointment.OwnerId != userId.Value || appointment.VetId != vetId)
            {
                TempData["ErrorMessage"] = "Booking not found or cannot rate this vet.";
                return RedirectToAction(nameof(Index));
            }
            await _ratingApiService.SubmitVetRatingAsync(vetId, userId.Value, bookingId, rating, comment);
            TempData["SuccessMessage"] = "Thank you for rating this vet.";
            return RedirectToAction(nameof(Details), new { id = appointmentId });
        }
        catch
        {
            TempData["ErrorMessage"] = "Could not submit rating. Please try again.";
            return RedirectToAction(nameof(Details), new { id = appointmentId });
        }
    }

    /// <summary>
    /// Demo consultation (full Vet Doctor Flow) – no booking required.
    /// </summary>
    [HttpGet]
    [Route("Consultations/Demo")]
    public IActionResult Demo()
    {
        var details = ConsultationStore.GetDetails(0, "BK-DEMO", null, null, null, null);
        if (details == null)
        {
            TempData["ErrorMessage"] = "Demo not available.";
            return RedirectToAction("Index", "Home");
        }
        return View("Details", details);
    }
}
