using System.Text;
using System.Text.Json;
using PawNect.PetParent.Web.Models;

namespace PawNect.PetParent.Web.Services;

/// <summary>
/// Calls PawNect API to get/submit ratings (stored in database). Replaces in-memory RatingStore.
/// </summary>
public class RatingApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _baseUrl;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RatingApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _baseUrl = (configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "http://localhost:5000/api").TrimEnd('/');
    }

    public async Task<double> GetAverageRatingForVetAsync(int vetId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_baseUrl}/ratings/vet/{vetId}/average");
            if (!response.IsSuccessStatusCode) return 0;
            var json = await response.Content.ReadAsStringAsync();
            var api = JsonSerializer.Deserialize<ApiResponseViewModel<double>>(json, JsonOptions);
            return api?.Success == true && api.Data is double d ? d : 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<bool> HasParentRatedBookingAsync(int parentUserId, string bookingId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}/ratings/vet/has-rated?parentUserId={Uri.EscapeDataString(parentUserId.ToString())}&bookingId={Uri.EscapeDataString(bookingId)}";
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return false;
            var json = await response.Content.ReadAsStringAsync();
            var api = JsonSerializer.Deserialize<ApiResponseViewModel<bool>>(json, JsonOptions);
            return api?.Success == true && api.Data == true;
        }
        catch
        {
            return false;
        }
    }

    public async Task SubmitVetRatingAsync(int vetId, int parentUserId, string bookingId, int rating, string? comment)
    {
        var client = _httpClientFactory.CreateClient();
        var body = new { vetId, parentUserId, bookingId, rating, comment };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        await client.PostAsync($"{_baseUrl}/ratings/vet", content);
    }

    public async Task<ParentRatingDto?> GetParentRatingByBookingAsync(int vetId, string bookingId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}/ratings/parent?vetId={vetId}&bookingId={Uri.EscapeDataString(bookingId)}";
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            var api = JsonSerializer.Deserialize<ApiResponseViewModel<ParentRatingDto>>(json, JsonOptions);
            return api?.Success == true ? api.Data : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task SubmitParentRatingAsync(int parentUserId, int vetId, string bookingId, int rating, string? comment)
    {
        var client = _httpClientFactory.CreateClient();
        var body = new { parentUserId, vetId, bookingId, rating, comment };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        await client.PostAsync($"{_baseUrl}/ratings/parent", content);
    }
}

/// <summary>DTO for parent rating from API (vet rates parent).</summary>
public class ParentRatingDto
{
    public int Id { get; set; }
    public int ParentUserId { get; set; }
    public int VetId { get; set; }
    public string BookingId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
