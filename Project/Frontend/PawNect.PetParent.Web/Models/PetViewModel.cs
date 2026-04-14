using System.ComponentModel.DataAnnotations;

namespace PawNect.PetParent.Web.Models;

/// <summary>
/// ViewModel for displaying pet information
/// </summary>
public class PetViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public string Species { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public double? WeightKg { get; set; }
    public string? Color { get; set; }
    public string? MicrochipId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string? Description { get; set; }
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public int Age => DateTime.Now.Year - DateOfBirth.Year;
}

/// <summary>
/// ViewModel for creating a new pet
/// </summary>
public class CreatePetViewModel
{
    [Required(ErrorMessage = "Pet name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    [Display(Name = "Pet Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Breed")]
    public string? Breed { get; set; }

    [Required(ErrorMessage = "Species is required")]
    [Display(Name = "Species")]
    public int Species { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Range(0.1, 200, ErrorMessage = "Weight must be between 0.1 and 200 kg")]
    [Display(Name = "Weight (kg)")]
    public double? WeightKg { get; set; }

    [StringLength(30)]
    [Display(Name = "Color")]
    public string? Color { get; set; }

    [StringLength(50)]
    [Display(Name = "Microchip ID")]
    public string? MicrochipId { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL")]
    [Display(Name = "Profile Image URL")]
    public string? ProfileImageUrl { get; set; }

    [StringLength(500)]
    [Display(Name = "Description")]
    public string? Description { get; set; }
}

/// <summary>
/// ViewModel for editing an existing pet
/// </summary>
public class EditPetViewModel : CreatePetViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "Status")]
    public int Status { get; set; }
}

/// <summary>
/// Enum helper for pet species
/// </summary>
public static class PetSpeciesHelper
{
    public static readonly Dictionary<int, string> Species = new()
    {
        { 0, "Dog" },
        { 1, "Cat" },
        { 2, "Bird" },
        { 3, "Fish" },
        { 4, "Rabbit" },
        { 5, "Hamster" },
        { 6, "Other" }
    };

    public static string GetSpeciesName(int speciesId) =>
        Species.TryGetValue(speciesId, out var name) ? name : "Unknown";
}

/// <summary>
/// Enum helper for pet status
/// </summary>
public static class PetStatusHelper
{
    public static readonly Dictionary<int, string> Statuses = new()
    {
        { 0, "Active" },
        { 1, "Inactive" },
        { 2, "Archived" }
    };

    public static string GetStatusName(int statusId) =>
        Statuses.TryGetValue(statusId, out var name) ? name : "Unknown";
}
