using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PawNect.PetParent.Web.Infrastructure;
using PawNect.PetParent.Web.Models;
using PawNect.PetParent.Web.Services;

namespace PawNect.PetParent.Web.Controllers;

public class VetsController : Controller
{
    private const int VetRole = 2; // VeterinaryClinic
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PawNect.PetParent.Web.Services.RatingApiService _ratingApiService;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public VetsController(IConfiguration configuration, IHttpClientFactory httpClientFactory, PawNect.PetParent.Web.Services.RatingApiService ratingApiService)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _ratingApiService = ratingApiService;
    }

    /// <summary>
    /// A2. Search & Discover Vets – no login required. mode=online for Online Consultation flow (same listing, different copy/CTA).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        string? sortBy,
        bool sortDesc = false,
        string? mode = null)
    {
        var isOnline = string.Equals(mode, ConsultMode.Online, StringComparison.OrdinalIgnoreCase);
        var vets = await GetVetsFromApiAsync();
        foreach (var v in vets)
        {
            var avg = await _ratingApiService.GetAverageRatingForVetAsync(v.Id);
            if (avg > 0) v.Rating = avg;
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            vets = vets.Where(v =>
                (v.Name?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (v.ClinicName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (v.ClinicLocation?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }
        vets = (sortBy?.ToLowerInvariant() switch
        {
            "name" => sortDesc ? vets.OrderByDescending(v => v.Name) : vets.OrderBy(v => v.Name),
            "clinic" => sortDesc ? vets.OrderByDescending(v => v.ClinicName) : vets.OrderBy(v => v.ClinicName),
            "rating" => sortDesc ? vets.OrderBy(v => v.Rating) : vets.OrderByDescending(v => v.Rating),
            "fee" => sortDesc ? vets.OrderByDescending(v => v.ConsultationFee) : vets.OrderBy(v => v.ConsultationFee),
            "experience" => sortDesc ? vets.OrderBy(v => v.ExperienceYears) : vets.OrderByDescending(v => v.ExperienceYears),
            _ => vets.OrderBy(v => v.ClinicName)
        }).ToList();
        ViewBag.Search = search;
        ViewBag.SortBy = sortBy ?? "clinic";
        ViewBag.SortDesc = sortDesc;
        ViewBag.ConsultMode = isOnline ? ConsultMode.Online : ConsultMode.InClinic;
        return View(vets);
    }

    /// <summary>
    /// A3. Vet Profile Page – login required to view details and book. mode=online for Online Consultation CTA.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id, string? mode = null)
    {
        if (!SessionHelper.GetUserId(HttpContext.Session).HasValue)
        {
            TempData["ErrorMessage"] = "Please sign in to view vet details and book appointments.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Details), new { id, mode }) });
        }
        var vet = await GetVetByIdFromApiAsync(id);
        if (vet == null)
        {
            TempData["ErrorMessage"] = "Vet not found.";
            return RedirectToAction(nameof(Index), new { mode });
        }
        var avgRating = await _ratingApiService.GetAverageRatingForVetAsync(vet.Id);
        if (avgRating > 0) vet.Rating = avgRating;
        ViewBag.ConsultMode = string.Equals(mode, ConsultMode.Online, StringComparison.OrdinalIgnoreCase) ? ConsultMode.Online : ConsultMode.InClinic;
        return View(vet);
    }

    /// <summary>
    /// A4. Slot Selection – login required. mode=online for Online Consultation flow.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SlotSelect(int vetId, string? mode = null)
    {
        if (!SessionHelper.GetUserId(HttpContext.Session).HasValue)
        {
            TempData["ErrorMessage"] = "Please sign in to book an appointment.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(SlotSelect), new { vetId, mode }) });
        }
        if (!SessionHelper.IsParent(HttpContext.Session))
        {
            TempData["ErrorMessage"] = "Booking is for pet parents. Please sign in as Pet Parent.";
            return RedirectToAction("Index", "Home");
        }
        var vet = await GetVetByIdFromApiAsync(vetId);
        if (vet == null)
        {
            TempData["ErrorMessage"] = "Vet not found.";
            return RedirectToAction(nameof(Index), new { mode });
        }
        var slots = SlotService.GetSlotsForVet(vetId);
        ViewBag.Vet = vet;
        ViewBag.ConsultMode = string.Equals(mode, ConsultMode.Online, StringComparison.OrdinalIgnoreCase) ? ConsultMode.Online : ConsultMode.InClinic;
        return View(slots);
    }

    /// <summary>
    /// A5. Verify Details – GET: show form with pet dropdown. mode=online adds symptoms field and sets ConsultMode.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Verify(int vetId, string date, string time, string? mode = null)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (!userId.HasValue)
        {
            TempData["ErrorMessage"] = "Please sign in to book.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(SlotSelect), new { vetId, mode }) });
        }
        if (!SessionHelper.IsParent(HttpContext.Session))
        {
            TempData["ErrorMessage"] = "Booking is for pet parents. Please sign in as Pet Parent.";
            return RedirectToAction("Index", "Home");
        }
        var vet = await GetVetByIdFromApiAsync(vetId);
        if (vet == null)
        {
            TempData["ErrorMessage"] = "Vet not found.";
            return RedirectToAction(nameof(Index), new { mode });
        }
        var slots = SlotService.GetSlotsForVet(vetId);
        var slot = slots.FirstOrDefault(s => s.Date == date && s.Time == time);
        if (slot == null || !slot.IsAvailable)
        {
            TempData["ErrorMessage"] = "Selected slot is not available.";
            return RedirectToAction(nameof(SlotSelect), new { vetId, mode });
        }
        var isOnline = string.Equals(mode, ConsultMode.Online, StringComparison.OrdinalIgnoreCase);
        var pets = await GetUserPetsAsync(userId.Value);
        var model = new BookingVerifyViewModel
        {
            VetId = vetId,
            VetName = vet.Name,
            ClinicName = vet.ClinicName,
            ClinicAddress = vet.ClinicAddress,
            SlotDate = date,
            SlotTime = time,
            SlotDisplay = slot.DisplayText,
            PetParentName = SessionHelper.GetUserName(HttpContext.Session),
            PetParentEmail = SessionHelper.GetUserEmail(HttpContext.Session),
            Pets = pets,
            SelectedPetId = pets.FirstOrDefault()?.Id ?? 0,
            ConsultMode = isOnline ? ConsultMode.Online : ConsultMode.InClinic
        };
        return View(model);
    }

    /// <summary>
    /// A5/A6. Verify POST → create booking, redirect to Confirmation.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmBooking(BookingVerifyViewModel model)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (!userId.HasValue)
        {
            TempData["ErrorMessage"] = "Please sign in to book.";
            return RedirectToAction("Login", "Account");
        }
        if (!SessionHelper.IsParent(HttpContext.Session))
        {
            TempData["ErrorMessage"] = "Booking is for pet parents. Please sign in as Pet Parent.";
            return RedirectToAction("Index", "Home");
        }
        var petName = model.Pets.FirstOrDefault(p => p.Id == model.SelectedPetId)?.Name ?? "Pet";
        var booking = new BookingConfirmViewModel
        {
            VetId = model.VetId,
            VetName = model.VetName,
            ClinicName = model.ClinicName,
            ClinicAddress = model.ClinicAddress,
            SlotDate = model.SlotDate,
            SlotTime = model.SlotTime,
            PetName = petName,
            Status = "Booked",
            ReasonForVisit = model.ReasonForVisit,
            PetId = model.SelectedPetId,
            ParentName = model.PetParentName ?? SessionHelper.GetUserName(HttpContext.Session),
            ParentEmail = model.PetParentEmail ?? SessionHelper.GetUserEmail(HttpContext.Session),
            ParentPhone = model.PetParentPhone
        };

        // Persist appointment to database via API
        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
        if (!DateTime.TryParseExact($"{model.SlotDate} {model.SlotTime}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startTime))
            startTime = DateTime.Parse($"{model.SlotDate} {model.SlotTime}", CultureInfo.InvariantCulture);
        var endTime = startTime.AddMinutes(30);
        var isOnline = string.Equals(model.ConsultMode, ConsultMode.Online, StringComparison.OrdinalIgnoreCase);
        var appointmentType = isOnline ? "Online" : "Vet";
        var notes = string.IsNullOrWhiteSpace(model.Symptoms)
            ? model.ReasonForVisit
            : string.IsNullOrWhiteSpace(model.ReasonForVisit)
                ? model.Symptoms
                : $"{model.ReasonForVisit}. Symptoms: {model.Symptoms}";
        var createDto = new
        {
            petId = model.SelectedPetId,
            title = string.IsNullOrWhiteSpace(model.ReasonForVisit) ? (isOnline ? $"Online consult - {model.ClinicName}" : $"Vet visit - {model.ClinicName}") : model.ReasonForVisit,
            appointmentType,
            startTime,
            endTime,
            location = model.ClinicAddress ?? model.ClinicName,
            providerName = model.VetName,
            providerContact = (string?)null,
            notes,
            vetId = model.VetId,
            ownerId = userId.Value
        };
        try
        {
            var client = _httpClientFactory.CreateClient();
            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{baseUrl}/appointments", content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<ApiResponseViewModel<object>>(errorContent, _jsonOptions);
                TempData["ErrorMessage"] = errorResponse?.Message ?? "Failed to save appointment. Please try again.";
                return RedirectToAction(nameof(Verify), new { vetId = model.VetId, date = model.SlotDate, time = model.SlotTime, mode = model.ConsultMode });
            }
    var successContent = await response.Content.ReadAsStringAsync();
            var createResponse = JsonSerializer.Deserialize<ApiResponseViewModel<AppointmentCreatedData>>(successContent, _jsonOptions);
            if (createResponse?.Data?.Id is int appointmentId)
            {
                return RedirectToAction(nameof(Confirmation), new { id = appointmentId });
            }
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Unable to save appointment. Please check the connection and try again.";
            return RedirectToAction(nameof(Verify), new { vetId = model.VetId, date = model.SlotDate, time = model.SlotTime, mode = model.ConsultMode });
        }

        TempData["ErrorMessage"] = "Appointment was created but confirmation could not be loaded. Check My Appointments.";
        return RedirectToAction("Index", "Appointments");
    }

    /// <summary>
    /// A6. Confirmation page (loads from API by appointment id).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Confirmation(int? id)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (!userId.HasValue)
        {
            TempData["ErrorMessage"] = "Please sign in to view confirmation.";
            return RedirectToAction("Login", "Account");
        }
        if (!id.HasValue)
        {
            TempData["ErrorMessage"] = "Booking not specified.";
            return RedirectToAction("Index", "Appointments");
        }
        var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{baseUrl}/appointments/{id.Value}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("Index", "Appointments");
            }
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponseViewModel<AppointmentApiDto>>(content, _jsonOptions);
            var appointment = apiResponse?.Data;
            if (appointment == null || appointment.OwnerId != userId.Value)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("Index", "Appointments");
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
                PetName = "", // optional on confirmation
                Status = appointment.Status ?? "Booked"
            };
            return View(booking);
        }
        catch
        {
            TempData["ErrorMessage"] = "Could not load confirmation. Check My Appointments.";
            return RedirectToAction("Index", "Appointments");
        }
    }

    private async Task<List<VetViewModel>> GetVetsFromApiAsync()
    {
        try
        {
            var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{baseUrl}/users/byrole/{VetRole}");
            if (!response.IsSuccessStatusCode) return new List<VetViewModel>();
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponseViewModel<List<UserApiDto>>>(content, _jsonOptions);
            if (apiResponse?.Success != true || apiResponse.Data == null) return new List<VetViewModel>();
            return apiResponse.Data.Select(MapUserToVetViewModel).ToList();
        }
        catch
        {
            return new List<VetViewModel>();
        }
    }

    private async Task<VetViewModel?> GetVetByIdFromApiAsync(int id)
    {
        try
        {
            var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{baseUrl}/users/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponseViewModel<UserApiDto>>(content, _jsonOptions);
            if (apiResponse?.Success != true || apiResponse.Data == null || apiResponse.Data.Role != VetRole) return null;
            return MapUserToVetViewModel(apiResponse.Data);
        }
        catch
        {
            return null;
        }
    }

    private static VetViewModel MapUserToVetViewModel(UserApiDto u)
    {
        var address = new[] { u.Address, u.City, u.State, u.ZipCode }.Where(s => !string.IsNullOrWhiteSpace(s));
        var clinicAddress = string.Join(", ", address);
        if (string.IsNullOrWhiteSpace(clinicAddress)) clinicAddress = "—";
        return new VetViewModel
        {
            Id = u.Id,
            Name = $"{u.FirstName} {u.LastName}".Trim(),
            ClinicName = u.OrganizationName ?? "—",
            ClinicAddress = clinicAddress,
            ClinicLocation = u.City ?? "—",
            Specialization = "Veterinary Clinic",
            Degree = "",
            ExperienceYears = 0,
            ConsultationFee = 0,
            Rating = 0,
            AvailabilityIndicator = "Contact for availability",
            AvailableToday = false,
            AvailableTomorrow = false,
            Bio = null,
            Qualifications = null,
            AreasOfExpertise = null,
            PracticeRegistrationNumber = null,
            TypicalCasesHandled = null,
            WeeklyAvailability = "Contact clinic for hours"
        };
    }

    private async Task<List<PetOptionViewModel>> GetUserPetsAsync(int ownerId)
    {
        try
        {
            var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "http://localhost:5000/api";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{baseUrl}/pets/owner/{ownerId}");
            if (!response.IsSuccessStatusCode) return new List<PetOptionViewModel>();
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponseViewModel<List<VetPetApiDto>>>(content, _jsonOptions);
            if (apiResponse?.Success != true || apiResponse.Data == null) return new List<PetOptionViewModel>();
            return apiResponse.Data.Select(p => new PetOptionViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Species = PetSpeciesHelper.GetSpeciesName(p.Species)
            }).ToList();
        }
        catch
        {
            return new List<PetOptionViewModel>();
        }
    }
}

internal class VetPetApiDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Species { get; set; }
}

internal class AppointmentCreatedData
{
    public int Id { get; set; }
}
