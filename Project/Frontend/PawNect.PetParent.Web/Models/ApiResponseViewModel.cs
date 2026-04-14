using System.Text.Json.Serialization;

namespace PawNect.PetParent.Web.Models;

/// <summary>
/// Generic API response wrapper for deserializing API responses
/// </summary>
public class ApiResponseViewModel<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<string>? Errors { get; set; }
}
