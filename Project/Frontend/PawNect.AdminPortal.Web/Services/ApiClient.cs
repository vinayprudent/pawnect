using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using PawNect.Application.DTOs;
using PawNect.Application.DTOs.User;

namespace PawNect.AdminPortal.Web.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ApiClient(HttpClient http, ILogger<ApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<ApiResponse<UserDto>?> LoginAsync(string email, string password)
    {
        var dto = new LoginUserDto { EmailOrMobile = email };
        var res = await _http.PostAsJsonAsync("api/users/login", dto, JsonOptions);
        return await ReadAsApiResponse<UserDto>(res);
    }

    public async Task<ApiResponse<IEnumerable<UserDto>>?> GetUsersByRoleAsync(int role)
    {
        var res = await _http.GetAsync($"api/users/byrole/{role}");
        return await ReadAsApiResponse<IEnumerable<UserDto>>(res);
    }

    public async Task<ApiResponse<UserDto>?> GetUserByIdAsync(int id)
    {
        var res = await _http.GetAsync($"api/users/{id}");
        return await ReadAsApiResponse<UserDto>(res);
    }

    public async Task<ApiResponse<UserDto>?> RegisterUserAsync(RegisterUserDto dto)
    {
        var res = await _http.PostAsJsonAsync("api/users/register", dto, JsonOptions);
        return await ReadAsApiResponse<UserDto>(res);
    }

    public async Task<ApiResponse<UserDto>?> UpdateUserAsync(int id, UserDto dto)
    {
        var res = await _http.PutAsJsonAsync($"api/users/{id}", dto, JsonOptions);
        return await ReadAsApiResponse<UserDto>(res);
    }

    public async Task<ApiResponse?> DeleteUserAsync(int id)
    {
        var res = await _http.DeleteAsync($"api/users/{id}");
        var json = await res.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(json)) return new ApiResponse { Success = res.IsSuccessStatusCode, Message = res.IsSuccessStatusCode ? "Deleted" : "Error" };
        return JsonSerializer.Deserialize<ApiResponse>(json, JsonOptions);
    }

    private static async Task<ApiResponse<T>?> ReadAsApiResponse<T>(HttpResponseMessage res)
    {
        var json = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse<T>>(json, JsonOptions);
    }
}
