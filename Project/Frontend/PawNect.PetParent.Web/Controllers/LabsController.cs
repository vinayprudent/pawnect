using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PawNect.PetParent.Web.Models;

namespace PawNect.PetParent.Web.Controllers;

/// <summary>
/// Find Labs – lists registered laboratories from API (no dummy data).
/// </summary>
public class LabsController : Controller
{
    private const int LabRole = 6; // Laboratory
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public LabsController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        var labs = await GetLabsFromApiAsync();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            labs = labs.Where(l =>
                (l.Name?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (l.LabName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (l.City?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }
        ViewBag.Search = search;
        return View(labs);
    }

    private async Task<List<LabViewModel>> GetLabsFromApiAsync()
    {
        try
        {
            var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5000/api";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{baseUrl}/users/byrole/{LabRole}");
            if (!response.IsSuccessStatusCode) return new List<LabViewModel>();
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponseViewModel<List<UserApiDto>>>(content, _jsonOptions);
            if (apiResponse?.Success != true || apiResponse.Data == null) return new List<LabViewModel>();
            return apiResponse.Data.Select(u => new LabViewModel
            {
                Id = u.Id,
                Name = $"{u.FirstName} {u.LastName}".Trim(),
                LabName = u.OrganizationName ?? "—",
                Email = u.Email,
                PhoneNumber = string.IsNullOrWhiteSpace(u.PhoneNumber) ? null : u.PhoneNumber,
                Address = u.Address,
                City = u.City,
                State = u.State,
                ZipCode = u.ZipCode
            }).ToList();
        }
        catch
        {
            return new List<LabViewModel>();
        }
    }
}
