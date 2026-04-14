using Microsoft.EntityFrameworkCore;
using PawNect.Application.Interfaces;
using PawNect.Domain.Entities;
using PawNect.Infrastructure.DbContext;

namespace PawNect.Infrastructure.Repositories;

/// <summary>
/// User Repository implementation
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(PawNectDbContext context) : base(context)
    {
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<User?> GetUserByEmailOrPhoneAsync(string emailOrPhone)
    {
        var value = (emailOrPhone ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalizedEmail = value.ToLowerInvariant();
        var normalizedPhone = NormalizePhoneForCompare(value);

        var users = await _dbSet.Where(u => !u.IsDeleted).ToListAsync();
        foreach (var user in users)
        {
            var emailMatch = !string.IsNullOrWhiteSpace(user.Email) &&
                             string.Equals(user.Email.Trim(), normalizedEmail, StringComparison.OrdinalIgnoreCase);
            if (emailMatch) 
                return user;

            var userPhone = NormalizePhoneForCompare(user.PhoneNumber ?? string.Empty);
            if (string.IsNullOrWhiteSpace(userPhone) || string.IsNullOrWhiteSpace(normalizedPhone))
                continue;

            if (userPhone == normalizedPhone)
                return user;

            // Support common mismatch where stored number includes country code but user enters local number.
            var userLast10 = userPhone.Length > 10 ? userPhone[^10..] : userPhone;
            var inputLast10 = normalizedPhone.Length > 10 ? normalizedPhone[^10..] : normalizedPhone;
            if (userLast10 == inputLast10) 
                return user;
        }
        return null;
    }

    private static string NormalizePhoneForCompare(string input)
    {
        return input
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace("(", string.Empty)
            .Replace(")", string.Empty)
            .Replace("+", string.Empty)
            .Trim();
    }

    public async Task<User?> GetUserWithPetsAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.OwnedPets)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(int role)
    {
        return await _dbSet
            .Where(u => (int)u.Role == role && !u.IsDeleted)
            .ToListAsync();
    }
}
