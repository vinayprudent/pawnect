namespace PawNect.Application.DTOs.Pet;

/// <summary>
/// DTO for updating an existing pet
/// </summary>
public class UpdatePetDto
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
}
