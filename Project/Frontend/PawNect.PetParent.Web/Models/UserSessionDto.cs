using System.Text.Json.Serialization;

namespace PawNect.PetParent.Web.Models;

/// <summary>
/// User data returned from API login for session storage
/// </summary>
internal class UserSessionDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public int Role { get; set; }
}
