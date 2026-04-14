using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PawNect.PetParent.Web.Infrastructure;
using PawNect.PetParent.Web.Models;

namespace PawNect.PetParent.Web.Controllers;

[RequireParentRole]
public class PetsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PetsController> _logger;
    private readonly string _apiBaseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public PetsController(IHttpClientFactory httpClientFactory, ILogger<PetsController> logger, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "http://localhost:5000/api";
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    // GET: Pets
    public async Task<IActionResult> Index()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var ownerId = SessionHelper.GetUserId(HttpContext.Session);
            var url = ownerId.HasValue
                ? $"{_apiBaseUrl}/pets/owner/{ownerId.Value}"
                : $"{_apiBaseUrl}/pets";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponseViewModel<IEnumerable<PetApiDto>>>(content, _jsonOptions);

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    var pets = apiResponse.Data.Select(MapToPetViewModel).ToList();
                    return View(pets);
                }
            }

            return View(new List<PetViewModel>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pets");
            TempData["ErrorMessage"] = "Unable to load pets. Please try again later.";
            return View(new List<PetViewModel>());
        }
    }

    // GET: Pets/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/pets/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponseViewModel<PetApiDto>>(content, _jsonOptions);

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    var pet = MapToPetViewModel(apiResponse.Data);
                    return View(pet);
                }
            }

            TempData["ErrorMessage"] = "Pet not found.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pet details for {PetId}", id);
            TempData["ErrorMessage"] = "Unable to load pet details. Please try again later.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Pets/Create
    public IActionResult Create()
    {
        var ownerId = SessionHelper.GetUserId(HttpContext.Session);
        if (!ownerId.HasValue)
        {
            TempData["ErrorMessage"] = "Please sign in to add a pet.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Create)) });
        }
        return View(new CreatePetViewModel
        {
            DateOfBirth = DateTime.Today.AddYears(-1)
        });
    }

    // POST: Pets/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePetViewModel model)
    {
        var ownerId = SessionHelper.GetUserId(HttpContext.Session);
        if (!ownerId.HasValue)
        {
            TempData["ErrorMessage"] = "Please sign in to add a pet.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Create)) });
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            
            var createDto = new
            {
                name = model.Name,
                breed = model.Breed,
                species = model.Species,
                dateOfBirth = model.DateOfBirth,
                weightKg = model.WeightKg,
                color = model.Color,
                microchipId = model.MicrochipId,
                profileImageUrl = model.ProfileImageUrl,
                description = model.Description,
                ownerId = ownerId.Value
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync($"{_apiBaseUrl}/pets", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Pet added successfully!";
                return RedirectToAction(nameof(Index));
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonSerializer.Deserialize<ApiResponseViewModel<object>>(errorContent, _jsonOptions);
            TempData["ErrorMessage"] = errorResponse?.Message ?? "Failed to add pet. Please try again.";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pet");
            TempData["ErrorMessage"] = "Unable to add pet. Please try again later.";
            return View(model);
        }
    }

    // GET: Pets/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/pets/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponseViewModel<PetApiDto>>(content, _jsonOptions);

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    var pet = apiResponse.Data;
                    var editModel = new EditPetViewModel
                    {
                        Id = pet.Id,
                        Name = pet.Name,
                        Breed = pet.Breed,
                        Species = pet.Species,
                        DateOfBirth = pet.DateOfBirth,
                        WeightKg = pet.WeightKg,
                        Color = pet.Color,
                        MicrochipId = pet.MicrochipId,
                        ProfileImageUrl = pet.ProfileImageUrl,
                        Description = pet.Description,
                        Status = pet.Status
                    };
                    return View(editModel);
                }
            }

            TempData["ErrorMessage"] = "Pet not found.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pet for edit {PetId}", id);
            TempData["ErrorMessage"] = "Unable to load pet. Please try again later.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Pets/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditPetViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            
            var updateDto = new
            {
                id = model.Id,
                name = model.Name,
                breed = model.Breed,
                species = model.Species,
                dateOfBirth = model.DateOfBirth,
                weightKg = model.WeightKg,
                color = model.Color,
                microchipId = model.MicrochipId,
                profileImageUrl = model.ProfileImageUrl,
                description = model.Description,
                status = model.Status
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await client.PutAsync($"{_apiBaseUrl}/pets/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Pet updated successfully!";
                return RedirectToAction(nameof(Details), new { id });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonSerializer.Deserialize<ApiResponseViewModel<object>>(errorContent, _jsonOptions);
            TempData["ErrorMessage"] = errorResponse?.Message ?? "Failed to update pet. Please try again.";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating pet {PetId}", id);
            TempData["ErrorMessage"] = "Unable to update pet. Please try again later.";
            return View(model);
        }
    }

    // GET: Pets/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/pets/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponseViewModel<PetApiDto>>(content, _jsonOptions);

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    var pet = MapToPetViewModel(apiResponse.Data);
                    return View(pet);
                }
            }

            TempData["ErrorMessage"] = "Pet not found.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pet for delete {PetId}", id);
            TempData["ErrorMessage"] = "Unable to load pet. Please try again later.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Pets/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"{_apiBaseUrl}/pets/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Pet deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete pet. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting pet {PetId}", id);
            TempData["ErrorMessage"] = "Unable to delete pet. Please try again later.";
        }

        return RedirectToAction(nameof(Index));
    }

    private static PetViewModel MapToPetViewModel(PetApiDto dto)
    {
        return new PetViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Breed = dto.Breed,
            Species = PetSpeciesHelper.GetSpeciesName(dto.Species),
            DateOfBirth = dto.DateOfBirth,
            WeightKg = dto.WeightKg,
            Color = dto.Color,
            MicrochipId = dto.MicrochipId,
            Status = PetStatusHelper.GetStatusName(dto.Status),
            ProfileImageUrl = dto.ProfileImageUrl,
            Description = dto.Description,
            OwnerId = dto.OwnerId,
            CreatedAt = dto.CreatedAt
        };
    }
}

/// <summary>
/// Internal DTO for deserializing API responses
/// </summary>
internal class PetApiDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public int Species { get; set; }
    public DateTime DateOfBirth { get; set; }
    public double? WeightKg { get; set; }
    public string? Color { get; set; }
    public string? MicrochipId { get; set; }
    public int Status { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Description { get; set; }
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
