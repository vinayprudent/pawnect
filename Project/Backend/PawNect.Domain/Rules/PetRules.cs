namespace PawNect.Domain.Rules;

/// <summary>
/// Business rule validation for pet
/// </summary>
public static class PetRules
{
    public const int MaxPetNameLength = 100;
    public const int MaxBreedLength = 50;
    public const double MaxPetWeightKg = 200;
    public const double MinPetWeightKg = 0.1;

    public static class Validations
    {
        public static (bool IsValid, string Message) ValidatePetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Pet name is required.");

            if (name.Length > MaxPetNameLength)
                return (false, $"Pet name cannot exceed {MaxPetNameLength} characters.");

            return (true, "Valid");
        }

        public static (bool IsValid, string Message) ValidatePetWeight(double? weight)
        {
            if (!weight.HasValue)
                return (true, "Valid");

            if (weight < MinPetWeightKg || weight > MaxPetWeightKg)
                return (false, $"Pet weight must be between {MinPetWeightKg} and {MaxPetWeightKg} kg.");

            return (true, "Valid");
        }

        public static (bool IsValid, string Message) ValidateDateOfBirth(DateTime dob)
        {
            if (dob > DateTime.Today)
                return (false, "Date of birth cannot be in the future.");

            var age = DateTime.Today.Year - dob.Year;
            if (age > 50)
                return (false, "Pet age seems unrealistic.");

            return (true, "Valid");
        }
    }
}
