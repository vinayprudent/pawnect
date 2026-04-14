using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PawNect.PetParent.Web.Infrastructure;
using PawNect.PetParent.Web.Models;
using PawNect.PetParent.Web.Services;

namespace PawNect.PetParent.Web.Controllers;

/// <summary>
/// Vet portal: B1 Dashboard (today's + upcoming appointments from API), B2 Consult screen (from API/DB).
/// </summary>
[RequireVetRole]
public class VetPortalController : Controller
{
    private readonly RatingApiService _ratingApiService;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public VetPortalController(RatingApiService ratingApiService, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _ratingApiService = ratingApiService;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var vetId = SessionHelper.GetUserId(HttpContext.Session);
        if (!vetId.HasValue)
            return RedirectToAction("Login", "Account");
        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
        var client = _httpClientFactory.CreateClient();
        var today = DateTime.Today;
        var todayRes = await client.GetAsync($"{baseUrl}/appointments/vet/{vetId}?fromDate={today:yyyy-MM-dd}");
        var upcomingRes = await client.GetAsync($"{baseUrl}/appointments/vet/{vetId}?upcomingOnly=true");
        var todayList = await MapAppointmentListAsync(todayRes);
        var upcomingList = await MapAppointmentListAsync(upcomingRes);
        var model = new VetDashboardViewModel
        {
            TodayAppointments = todayList,
            UpcomingAppointments = upcomingList
        };
        return View(model);
    }

    private static async Task<List<VetAppointmentItemViewModel>> MapAppointmentListAsync(HttpResponseMessage r)
    {
        if (!r.IsSuccessStatusCode) return new List<VetAppointmentItemViewModel>();
        var content = await r.Content.ReadAsStringAsync();
        var wrap = JsonSerializer.Deserialize<ApiResponseViewModel<List<VetAppointmentItemViewModel>>>(content, JsonOptions);
        return wrap?.Data ?? new List<VetAppointmentItemViewModel>();
    }

    /// <summary>B2 list: all consultations for this vet (from API).</summary>
    [HttpGet]
    public async Task<IActionResult> Consultations()
    {
        var vetId = SessionHelper.GetUserId(HttpContext.Session);
        if (!vetId.HasValue)
            return RedirectToAction("Login", "Account");
        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync($"{baseUrl}/appointments/vet/{vetId}");
        var list = await MapAppointmentListAsync(response);
        return View(list);
    }

    /// <summary>B2 Consult screen: load from API by appointmentId.</summary>
    [HttpGet]
    public async Task<IActionResult> Consult(int appointmentId)
    {
        var vetId = SessionHelper.GetUserId(HttpContext.Session);
        if (!vetId.HasValue)
            return RedirectToAction("Login", "Account");
        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync($"{baseUrl}/consultations/appointment/{appointmentId}");
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Consultation not found.";
            return RedirectToAction(nameof(Consultations));
        }
        var content = await response.Content.ReadAsStringAsync();
        var wrap = JsonSerializer.Deserialize<ApiResponseViewModel<ConsultationApiDto>>(content, JsonOptions);
        var dto = wrap?.Data;
        if (dto == null)
        {
            TempData["ErrorMessage"] = "Consultation not found.";
            return RedirectToAction(nameof(Consultations));
        }
        if (dto.VetId != vetId.Value)
        {
            TempData["ErrorMessage"] = "Access denied.";
            return RedirectToAction(nameof(Consultations));
        }
        var previousRes = await client.GetAsync($"{baseUrl}/consultations/by-pet/{dto.PetId}?excludeAppointmentId={appointmentId}");
        var previousList = new List<PreviousConsultSummaryViewModel>();
        if (previousRes.IsSuccessStatusCode)
        {
            var prevContent = await previousRes.Content.ReadAsStringAsync();
            var prevWrap = JsonSerializer.Deserialize<ApiResponseViewModel<List<PreviousConsultApiDto>>>(prevContent, JsonOptions);
            if (prevWrap?.Data != null)
                previousList = prevWrap.Data.Select(p => new PreviousConsultSummaryViewModel { Date = p.Date, VetName = p.VetName, Summary = p.Summary }).ToList();
        }
        var details = MapToConsultationDetails(dto, previousList);
        if (dto.Id > 0)
        {
            var orderRes = await client.GetAsync($"{baseUrl}/diagnostics/orders/consultation/{dto.Id}");
            if (orderRes.IsSuccessStatusCode)
            {
                var orderContent = await orderRes.Content.ReadAsStringAsync();
                var orderWrap = JsonSerializer.Deserialize<ApiResponseViewModel<DiagnosticOrderApiDto>>(orderContent, JsonOptions);
                if (orderWrap?.Data != null)
                    details.DiagnosticOrder = new DiagnosticOrderViewModel
                    {
                        OrderId = orderWrap.Data.Id.ToString(),
                        Status = orderWrap.Data.Status,
                        TestsOrdered = orderWrap.Data.TestsOrdered ?? new List<string>(),
                        CollectionType = "In-Clinic",
                        TotalPrice = orderWrap.Data.TotalPrice,
                        AssignedLab = orderWrap.Data.AssignedLabName,
                        OrderedAt = orderWrap.Data.OrderedAt?.ToString("yyyy-MM-dd HH:mm"),
                        SampleCollectedAt = orderWrap.Data.SampleCollectedAt?.ToString("yyyy-MM-dd HH:mm"),
                        ReportUploadedAt = orderWrap.Data.ReportUploadedAt?.ToString("yyyy-MM-dd HH:mm")
                    };
            }
            var reportRes = await client.GetAsync($"{baseUrl}/diagnostics/reports/consultation/{dto.Id}");
            if (reportRes.IsSuccessStatusCode)
            {
                var reportContent = await reportRes.Content.ReadAsStringAsync();
                var reportWrap = JsonSerializer.Deserialize<ApiResponseViewModel<DiagnosticReportApiDto>>(reportContent, JsonOptions);
                if (reportWrap?.Data != null)
                {
                    var r = reportWrap.Data;
                    details.Report = new LabReportViewModel
                    {
                        ReportId = r.Id.ToString(),
                        ReportFileName = r.ReportFileName,
                        VetAdvice = r.VetAdvice,
                        NextSteps = r.NextSteps,
                        ReviewedAt = r.ReviewedAt?.ToString("g")
                    };
                }
            }
        }
        if (dto.OwnerId > 0)
        {
            var dto2 = await _ratingApiService.GetParentRatingByBookingAsync(vetId.Value, appointmentId.ToString());
            if (dto2 != null)
                ViewBag.ParentRating = new ParentRatingViewModel { Rating = dto2.Rating, Comment = dto2.Comment };
        }
        ViewBag.AppointmentId = appointmentId;
        ViewBag.ApiBaseUrl = baseUrl.Replace("/api", "");
        return View(details);
    }

    private static ConsultationDetailsViewModel MapToConsultationDetails(ConsultationApiDto dto, List<PreviousConsultSummaryViewModel> previousConsults)
    {
        var vitals = new List<VitalViewModel>();
        if (!string.IsNullOrEmpty(dto.VitalsJson))
        {
            try
            {
                var entries = JsonSerializer.Deserialize<List<VitalEntryDto>>(dto.VitalsJson);
                if (entries != null)
                    vitals = entries.Select(e => new VitalViewModel { Name = e.Name, Value = e.Value, Unit = e.Unit, RecordedAt = e.RecordedAt }).ToList();
            }
            catch { /* ignore */ }
        }
        return new ConsultationDetailsViewModel
        {
            BookingId = dto.AppointmentId.ToString(),
            ConsultationId = dto.Id,
            ConsultStatus = dto.Status,
            VetId = dto.VetId,
            ParentUserId = dto.OwnerId,
            PetId = dto.PetId,
            OwnerId = dto.OwnerId,
            PetName = dto.PetName ?? "",
            PetSpecies = dto.PetSpecies ?? "",
            PetBreed = dto.PetBreed,
            PetWeightKg = dto.PetWeightKg,
            ParentName = dto.ParentName,
            ParentEmail = dto.ParentEmail,
            ParentPhone = dto.ParentPhone,
            ReasonForVisit = dto.ReasonForVisit ?? "—",
            VetName = dto.VetName ?? "",
            ClinicName = dto.ClinicName ?? "",
            ClinicAddress = dto.ClinicAddress ?? "",
            SlotDate = dto.SlotStart?.ToString("yyyy-MM-dd") ?? "",
            SlotTime = dto.SlotStart?.ToString("HH:mm") ?? "",
            Notes = dto.Notes,
            ProvisionalDiagnosis = dto.ProvisionalDiagnosis,
            PrescriptionFileName = dto.PrescriptionUrl != null ? Path.GetFileName(dto.PrescriptionUrl) : null,
            PrescriptionUrl = dto.PrescriptionUrl,
            Vitals = vitals,
            ConsultationComplete = dto.ConsultationComplete,
            DiagnosticsRecommended = dto.DiagnosticsRecommended,
            PreviousConsults = previousConsults
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Consult(VetConsultUpdateViewModel model, IFormFile? PrescriptionFile)
    {
        var vetId = SessionHelper.GetUserId(HttpContext.Session);
        if (!vetId.HasValue)
            return RedirectToAction("Login", "Account");
        if (!int.TryParse(model.BookingId, out var appointmentId))
        {
            TempData["ErrorMessage"] = "Invalid appointment.";
            return RedirectToAction(nameof(Consultations));
        }
        if (model.VetId != vetId.Value)
        {
            TempData["ErrorMessage"] = "Access denied.";
            return RedirectToAction(nameof(Consultations));
        }

        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
        var client = _httpClientFactory.CreateClient();
        var getRes = await client.GetAsync($"{baseUrl}/consultations/appointment/{appointmentId}");
        var vitalsList = new List<VitalEntryDto>();
        if (getRes.IsSuccessStatusCode)
        {
            var getContent = await getRes.Content.ReadAsStringAsync();
            var getWrap = JsonSerializer.Deserialize<ApiResponseViewModel<ConsultationApiDto>>(getContent, JsonOptions);
            if (!string.IsNullOrEmpty(getWrap?.Data?.VitalsJson))
            {
                try
                {
                    var existing = JsonSerializer.Deserialize<List<VitalEntryDto>>(getWrap.Data.VitalsJson);
                    if (existing != null) vitalsList.AddRange(existing);
                }
                catch { /* ignore */ }
            }
        }
        if (!string.IsNullOrWhiteSpace(model.NewVitals))
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            foreach (var line in model.NewVitals.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Split('|', StringSplitOptions.TrimEntries);
                if (parts.Length >= 2)
                    vitalsList.Add(new VitalEntryDto { Name = parts[0], Value = parts[1], Unit = parts.Length >= 3 ? parts[2] : null, RecordedAt = now });
            }
        }
        var vitalsJson = vitalsList.Count > 0 ? JsonSerializer.Serialize(vitalsList) : null;

        string? newStatus = null;
        if (model.NextAction == "Completed" || (model.ConsultationComplete && model.NextAction != "InProgress"))
            newStatus = ConsultStatus.Completed;
        else if (model.NextAction == "InProgress")
            newStatus = ConsultStatus.InProgress;

        var saveDto = new
        {
            appointmentId,
            vetId = model.VetId,
            petId = model.PetId,
            ownerId = model.OwnerId,
            status = newStatus,
            notes = model.Notes,
            provisionalDiagnosis = model.ProvisionalDiagnosis,
            prescriptionUrl = (string?)null,
            vitalsJson,
            consultationComplete = model.ConsultationComplete,
            diagnosticsRecommended = model.DiagnosticsRecommended
        };
        var json = JsonSerializer.Serialize(saveDto);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var saveRes = await client.PostAsync($"{baseUrl}/consultations", content);
        if (!saveRes.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Failed to save consultation.";
            return RedirectToAction(nameof(Consult), new { appointmentId });
        }
        var saveContent = await saveRes.Content.ReadAsStringAsync();
        var saveWrap = JsonSerializer.Deserialize<ApiResponseViewModel<ConsultationApiDto>>(saveContent, JsonOptions);
        var consultationId = saveWrap?.Data?.Id ?? 0;
        if (consultationId > 0 && PrescriptionFile != null && PrescriptionFile.Length > 0)
        {
            using var form = new MultipartFormDataContent();
            using var stream = PrescriptionFile.OpenReadStream();
            form.Add(new StreamContent(stream), "file", PrescriptionFile.FileName);
            await client.PostAsync($"{baseUrl}/consultations/{consultationId}/prescription", form);
        }

        if (newStatus != null)
        {
            var updateAppointment = new { status = newStatus };
            var updateJson = JsonSerializer.Serialize(updateAppointment);
            var updateContent = new StringContent(updateJson, System.Text.Encoding.UTF8, "application/json");
            await client.PutAsync($"{baseUrl}/appointments/{appointmentId}", updateContent);
        }

        TempData["SuccessMessage"] = "Consultation updated.";
        return RedirectToAction(nameof(Consult), new { appointmentId });
    }

    /// <summary>Vet rates pet parent (1-5 stars, optional comment). Stored in database via API.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RateParent(string bookingId, int parentUserId, int rating, string? comment)
    {
        var vetId = SessionHelper.GetUserId(HttpContext.Session);
        if (!vetId.HasValue)
            return RedirectToAction("Login", "Account");
        var booking = BookingStore.GetByBookingId(bookingId);
        if (booking == null)
        {
            if (!int.TryParse(bookingId, out var appointmentId) || appointmentId <= 0)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction(nameof(Consultations));
            }
        }
        else if (booking.VetId != vetId.Value || booking.ParentUserId != parentUserId)
        {
            TempData["ErrorMessage"] = "Cannot rate this pet parent.";
            return RedirectToAction(nameof(Consultations));
        }
        await _ratingApiService.SubmitParentRatingAsync(parentUserId, vetId.Value, bookingId, rating, comment);
        TempData["SuccessMessage"] = "Rating saved.";
        if (int.TryParse(bookingId, out var aptId) && aptId > 0)
            return RedirectToAction(nameof(Consult), new { appointmentId = aptId });
        return RedirectToAction(nameof(Consultations));
    }

    [HttpGet]
    public async Task<IActionResult> OrderDiagnostics(int? appointmentId)
    {
        var vetId = SessionHelper.GetUserId(HttpContext.Session);
        if (!vetId.HasValue)
            return RedirectToAction("Login", "Account");
        if (!appointmentId.HasValue || appointmentId.Value <= 0)
        {
            ViewBag.Message = "Open a consult from Dashboard or Consultations, then use 'Order diagnostics' on that consult.";
            return View();
        }
        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
        var client = _httpClientFactory.CreateClient();
        var res = await client.GetAsync($"{baseUrl}/consultations/appointment/{appointmentId!.Value}");
        if (!res.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Load consultation first (save the consult for this appointment).";
            return RedirectToAction(nameof(Consult), new { appointmentId = appointmentId.Value });
        }
        var content = await res.Content.ReadAsStringAsync();
        var wrap = JsonSerializer.Deserialize<ApiResponseViewModel<ConsultationApiDto>>(content, JsonOptions);
        var consultation = wrap?.Data;
        if (consultation == null || consultation.Id == 0)
        {
            TempData["ErrorMessage"] = "Save the consultation first, then order diagnostics.";
            return RedirectToAction(nameof(Consult), new { appointmentId = appointmentId.Value });
        }
        var catalogRes = await client.GetAsync($"{baseUrl}/diagnostics/catalog");
        var catalog = new List<LabTestCatalogItemApiDto>();
        if (catalogRes.IsSuccessStatusCode)
        {
            var catalogContent = await catalogRes.Content.ReadAsStringAsync();
            var catalogWrap = JsonSerializer.Deserialize<ApiResponseViewModel<List<LabTestCatalogItemApiDto>>>(catalogContent, JsonOptions);
            catalog = catalogWrap?.Data ?? new List<LabTestCatalogItemApiDto>();
        }
        ViewBag.Catalog = catalog;
        ViewBag.ConsultationId = consultation.Id;
        ViewBag.AppointmentId = appointmentId.Value;
        ViewBag.PetId = consultation.PetId;
        ViewBag.OwnerId = consultation.OwnerId;
        ViewBag.VetId = consultation.VetId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OrderDiagnostics(int consultationId, int petId, int ownerId, int vetId, string? assignedLabName, string? selectedTestsJson)
    {
        var sessionVetId = SessionHelper.GetUserId(HttpContext.Session);
        if (!sessionVetId.HasValue || sessionVetId.Value != vetId)
        {
            TempData["ErrorMessage"] = "Access denied.";
            return RedirectToAction(nameof(Index));
        }
        List<SelectedTestDto>? selected;
        try
        {
            selected = string.IsNullOrEmpty(selectedTestsJson) ? null : JsonSerializer.Deserialize<List<SelectedTestDto>>(selectedTestsJson);
        }
        catch
        {
            selected = null;
        }
        if (selected == null || selected.Count == 0)
        {
            TempData["ErrorMessage"] = "Select at least one test.";
            return RedirectToAction(nameof(Index));
        }
        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
        var client = _httpClientFactory.CreateClient();
        var tests = selected.Select(t => new { testName = t.TestName, labTestCatalogItemId = t.TestId > 0 ? t.TestId : (int?)null, price = t.Price }).ToList();
        var body = new { consultationId, petId, ownerId, vetId, collectionType = "In-Clinic", assignedLabName = assignedLabName ?? "PawNect Lab – Auto-assigned", tests };
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"{baseUrl}/diagnostics/orders", content);
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Failed to create diagnostic order.";
            return RedirectToAction(nameof(Index));
        }
        TempData["SuccessMessage"] = "Diagnostic order created. Pet parent will see it in their consultation details.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Update diagnostic order status (Lab Flow — In-Clinic).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDiagnosticStatus(int diagnosticOrderId, string status, int appointmentId)
    {
        var vetId = SessionHelper.GetUserId(HttpContext.Session);
        if (!vetId.HasValue)
            return RedirectToAction("Login", "Account");
        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
        var client = _httpClientFactory.CreateClient();
        var body = JsonSerializer.Serialize(new { status });
        var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PatchAsync($"{baseUrl}/diagnostics/orders/{diagnosticOrderId}/status", content);
        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = "Diagnostic status updated.";
        else
            TempData["ErrorMessage"] = "Failed to update status.";
        return RedirectToAction(nameof(Consult), new { appointmentId });
    }

    /// <summary>Upload report and add vet advice / next steps (Report Flow).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadReport(int diagnosticOrderId, int appointmentId, string? vetAdvice, string? nextSteps, IFormFile? file)
    {
        var vetId = SessionHelper.GetUserId(HttpContext.Session);
        if (!vetId.HasValue)
            return RedirectToAction("Login", "Account");
        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
        var client = _httpClientFactory.CreateClient();
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(diagnosticOrderId.ToString()), "diagnosticOrderId");
        form.Add(new StringContent(vetAdvice ?? ""), "vetAdvice");
        form.Add(new StringContent(nextSteps ?? ""), "nextSteps");
        if (file != null && file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            form.Add(new StreamContent(stream), "file", file.FileName);
        }
        var response = await client.PostAsync($"{baseUrl}/diagnostics/reports", form);
        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = "Report and advice saved. Pet parent can see it in their consultation details.";
        else
            TempData["ErrorMessage"] = "Failed to save report.";
        return RedirectToAction(nameof(Consult), new { appointmentId });
    }

    [HttpGet]
    public IActionResult Reports()
    {
        return View();
    }

    [HttpGet]
    public IActionResult MedicalGuidance()
    {
        return View();
    }

    /// <summary>Vet sets own availability calendar: days ahead, slot duration (batch), max patients per slot. Defaults: 365 days, 60 min, 3 per slot.</summary>
    [HttpGet]
    public IActionResult Availability()
    {
        var vetId = SessionHelper.GetUserId(HttpContext.Session);
        if (!vetId.HasValue)
            return RedirectToAction("Login", "Account");
        var settings = VetAvailabilityStore.GetSettings(vetId.Value);
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Availability(VetAvailabilitySettings model)
    {
        var vetId = SessionHelper.GetUserId(HttpContext.Session);
        if (!vetId.HasValue)
            return RedirectToAction("Login", "Account");
        model.VetId = vetId.Value;
        if (model.DaysAhead < 1) model.DaysAhead = VetAvailabilitySettings.DefaultDaysAhead;
        if (model.SlotDurationMinutes < 15) model.SlotDurationMinutes = VetAvailabilitySettings.DefaultSlotDurationMinutes;
        if (model.MaxAppointmentsPerSlot < 1) model.MaxAppointmentsPerSlot = VetAvailabilitySettings.DefaultMaxAppointmentsPerSlot;
        if (model.StartHour < 0 || model.StartHour > 23) model.StartHour = VetAvailabilitySettings.DefaultStartHour;
        if (model.EndHour < 0 || model.EndHour > 24) model.EndHour = VetAvailabilitySettings.DefaultEndHour;
        if (model.EndHour <= model.StartHour) model.EndHour = model.StartHour + 8;
        VetAvailabilityStore.SetSettings(model);
        TempData["SuccessMessage"] = "Availability settings saved. Pet parents will see slots based on these settings.";
        return RedirectToAction(nameof(Availability));
    }
}
