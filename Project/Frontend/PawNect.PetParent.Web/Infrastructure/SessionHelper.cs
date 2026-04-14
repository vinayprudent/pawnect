namespace PawNect.PetParent.Web.Infrastructure;

/// <summary>
/// Login access selection: Parent (PetParent), Vet (VeterinaryClinic), Lab (Laboratory).
/// Matches backend UserRole enum values.
/// </summary>
public static class UserRoleConstants
{
    public const int Parent = 1;   // PetParent
    public const int Vet = 2;       // VeterinaryClinic
    public const int Lab = 6;       // Laboratory
}

public static class SessionHelper
{
    private const string UserIdKey = "UserId";
    private const string UserEmailKey = "UserEmail";
    private const string UserNameKey = "UserName";
    private const string UserRoleKey = "UserRole";

    public static int? GetUserId(ISession session) => session.GetInt32(UserIdKey);
    public static string? GetUserEmail(ISession session) => session.GetString(UserEmailKey);
    public static string? GetUserName(ISession session) => session.GetString(UserNameKey);
    public static int? GetUserRole(ISession session) => session.GetInt32(UserRoleKey);
    public static bool IsAuthenticated(ISession session) => session.GetInt32(UserIdKey).HasValue;

    public static bool IsParent(ISession session) => GetUserRole(session) == UserRoleConstants.Parent;
    public static bool IsVet(ISession session) => GetUserRole(session) == UserRoleConstants.Vet;
    public static bool IsLab(ISession session) => GetUserRole(session) == UserRoleConstants.Lab;
}
