using PawNect.Domain.Entities;

namespace PawNect.Application.Interfaces;

/// <summary>
/// User Repository interface
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByEmailOrPhoneAsync(string emailOrPhone);
    Task<User?> GetUserWithPetsAsync(int userId);
    Task<bool> EmailExistsAsync(string email);
    Task<IEnumerable<User>> GetUsersByRoleAsync(int role);
}
