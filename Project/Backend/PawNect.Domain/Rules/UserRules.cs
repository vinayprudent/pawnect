namespace PawNect.Domain.Rules;

/// <summary>
/// Business rule validation for user
/// </summary>
public static class UserRules
{
    public const int MaxNameLength = 100;
    public const int MinPasswordLength = 8;
    public const int MaxEmailLength = 255;

    public static class Validations
    {
        public static (bool IsValid, string Message) ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email is required.");

            if (email.Length > MaxEmailLength)
                return (false, $"Email cannot exceed {MaxEmailLength} characters.");

            if (!email.Contains("@"))
                return (false, "Email format is invalid.");

            return (true, "Valid");
        }

        public static (bool IsValid, string Message) ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Name is required.");

            if (name.Length > MaxNameLength)
                return (false, $"Name cannot exceed {MaxNameLength} characters.");

            return (true, "Valid");
        }

        public static (bool IsValid, string Message) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password is required.");

            if (password.Length < MinPasswordLength)
                return (false, $"Password must be at least {MinPasswordLength} characters.");

            return (true, "Valid");
        }
    }
}
