using PawNect.Application.DTOs.User;
using PawNect.Application.Interfaces;
using PawNect.Application.Settings;
using PawNect.Domain.Entities;
using PawNect.Domain.Enums;
using PawNect.Domain.Rules;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace PawNect.Application.Services;

/// <summary>
/// User Service implementation
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository<OtpChallenge> _otpRepository;
    private readonly INotificationSender _notificationSender;
    private readonly OtpSettings _otpSettings;

    public UserService(
        IUserRepository userRepository,
        IRepository<OtpChallenge> otpRepository,
        INotificationSender notificationSender,
        IOptions<OtpSettings> otpSettings)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _notificationSender = notificationSender;
        _otpSettings = otpSettings.Value;
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        return user == null ? null : MapToDto(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(int role)
    {
        var users = await _userRepository.GetUsersByRoleAsync(role);
        return users.Select(MapToDto);
    }

    public async Task<OtpChallengeResponseDto> InitiateRegistrationAsync(RegisterUserDto registerDto, CancellationToken cancellationToken = default)
    {
        ValidateRegistration(registerDto);
        if (await _userRepository.EmailExistsAsync(registerDto.Email))
            throw new InvalidOperationException("Email already registered.");

        var code = GenerateOtpCode(_otpSettings.Length);
        var now = DateTime.UtcNow;
        var challenge = new OtpChallenge
        {
            Purpose = OtpPurpose.Register,
            Channel = OtpChannel.Email,
            Destination = registerDto.Email.Trim().ToLowerInvariant(),
            CodeHash = HashPassword(code),
            ExpiresAt = now.AddMinutes(_otpSettings.ExpiryMinutes),
            MaxAttempts = _otpSettings.MaxAttempts,
            ResendAvailableAt = now.AddSeconds(_otpSettings.ResendCooldownSeconds),
            MetadataJson = JsonSerializer.Serialize(registerDto)
        };

        await _otpRepository.AddAsync(challenge);
        await _otpRepository.SaveChangesAsync();
        await _notificationSender.SendOtpAsync(OtpChannel.Email, challenge.Destination, code, OtpPurpose.Register, cancellationToken);

        return MapChallenge(challenge);
    }

    public async Task<UserDto> VerifyRegistrationOtpAsync(RegisterVerifyOtpDto verifyDto, CancellationToken cancellationToken = default)
    {
        var challenge = await GetActiveChallengeAsync(verifyDto.ChallengeId, OtpPurpose.Register);
        await ValidateOtpAttemptAsync(challenge, verifyDto.OtpCode);

        var registerDto = JsonSerializer.Deserialize<RegisterUserDto>(challenge.MetadataJson ?? string.Empty);
        if (registerDto == null)
            throw new InvalidOperationException("Registration data is not available for this OTP request.");
        ValidateRegistration(registerDto);
        if (await _userRepository.EmailExistsAsync(registerDto.Email))
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            PhoneNumber = registerDto.PhoneNumber,
            PasswordHash = HashPassword(Guid.NewGuid().ToString("N")),
            Role = (UserRole)registerDto.Role,
            Address = registerDto.Address,
            City = registerDto.City,
            State = registerDto.State,
            ZipCode = registerDto.ZipCode,
            OrganizationName = registerDto.OrganizationName,
            IsEmailVerified = true
        };
        var createdUser = await _userRepository.AddAsync(user);
        challenge.ConsumedAt = DateTime.UtcNow;
        challenge.UpdatedAt = DateTime.UtcNow;
        await _otpRepository.UpdateAsync(challenge);
        await _otpRepository.SaveChangesAsync();
        await _userRepository.SaveChangesAsync();
        return MapToDto(createdUser);
    }

    public async Task<OtpChallengeResponseDto> InitiateLoginAsync(LoginUserDto loginDto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(loginDto.EmailOrMobile))
            throw new ArgumentException("Email or mobile number is required.");

        var user = await _userRepository.GetUserByEmailOrPhoneAsync(loginDto.EmailOrMobile);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or mobile number.");

        var code = GenerateOtpCode(_otpSettings.Length);
        var now = DateTime.UtcNow;
        var challenge = new OtpChallenge
        {
            Purpose = OtpPurpose.Login,
            Channel = OtpChannel.Email,
            Destination = user.Email,
            UserId = user.Id,
            CodeHash = HashPassword(code),
            ExpiresAt = now.AddMinutes(_otpSettings.ExpiryMinutes),
            MaxAttempts = _otpSettings.MaxAttempts,
            ResendAvailableAt = now.AddSeconds(_otpSettings.ResendCooldownSeconds)
        };

        await _otpRepository.AddAsync(challenge);
        await _otpRepository.SaveChangesAsync();
        await _notificationSender.SendOtpAsync(OtpChannel.Email, challenge.Destination, code, OtpPurpose.Login, cancellationToken);
        return MapChallenge(challenge);
    }

    public async Task<UserDto?> VerifyLoginOtpAsync(LoginVerifyOtpDto verifyDto, CancellationToken cancellationToken = default)
    {
        var challenge = await GetActiveChallengeAsync(verifyDto.ChallengeId, OtpPurpose.Login);
        await ValidateOtpAttemptAsync(challenge, verifyDto.OtpCode);

        if (!challenge.UserId.HasValue)
            throw new InvalidOperationException("Invalid OTP challenge.");

        var user = await _userRepository.GetByIdAsync(challenge.UserId.Value);
        if (user == null || user.IsDeleted)
            throw new UnauthorizedAccessException("Unable to complete login.");

        challenge.ConsumedAt = DateTime.UtcNow;
        challenge.UpdatedAt = DateTime.UtcNow;
        user.LastLoginAt = DateTime.UtcNow;
        await _otpRepository.UpdateAsync(challenge);
        await _userRepository.UpdateAsync(user);
        await _otpRepository.SaveChangesAsync();
        await _userRepository.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task<OtpChallengeResponseDto> ResendOtpAsync(ResendOtpDto resendDto, CancellationToken cancellationToken = default)
    {
        var challenge = (await _otpRepository.FindAsync(c => c.ChallengeId == resendDto.ChallengeId && !c.IsDeleted))
            .FirstOrDefault();
        if (challenge == null || challenge.ConsumedAt.HasValue || challenge.ExpiresAt <= DateTime.UtcNow)
            throw new InvalidOperationException("OTP challenge is no longer active.");
        if (challenge.ResendAvailableAt > DateTime.UtcNow)
            throw new InvalidOperationException("Please wait before requesting another OTP.");

        var code = GenerateOtpCode(_otpSettings.Length);
        var now = DateTime.UtcNow;
        challenge.CodeHash = HashPassword(code);
        challenge.ExpiresAt = now.AddMinutes(_otpSettings.ExpiryMinutes);
        challenge.AttemptCount = 0;
        challenge.ResendAvailableAt = now.AddSeconds(_otpSettings.ResendCooldownSeconds);
        challenge.UpdatedAt = now;
        await _otpRepository.UpdateAsync(challenge);
        await _otpRepository.SaveChangesAsync();
        await _notificationSender.SendOtpAsync(challenge.Channel, challenge.Destination, code, challenge.Purpose, cancellationToken);
        return MapChallenge(challenge);
    }

    public async Task<bool> UpdateUserAsync(int userId, UserDto updateDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        user.FirstName = updateDto.FirstName;
        user.LastName = updateDto.LastName;
        user.PhoneNumber = updateDto.PhoneNumber;
        user.Address = updateDto.Address;
        user.City = updateDto.City;
        user.State = updateDto.State;
        user.ZipCode = updateDto.ZipCode;
        user.OrganizationName = updateDto.OrganizationName;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var success = await _userRepository.DeleteAsync(userId);
        if (success)
            await _userRepository.SaveChangesAsync();

        return success;
    }

    /// <summary>Public for seeding default admin. Use same algorithm as login.</summary>
    public static string HashPasswordForSeed(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private static string HashPassword(string password) => HashPasswordForSeed(password);

    private static bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput.Equals(hash);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = (int)user.Role,
            Address = user.Address,
            City = user.City,
            State = user.State,
            ZipCode = user.ZipCode,
            OrganizationName = user.OrganizationName,
            IsEmailVerified = user.IsEmailVerified,
            IsPhoneVerified = user.IsPhoneVerified,
            LastLoginAt = user.LastLoginAt
        };
    }

    private static string GenerateOtpCode(int length)
    {
        var random = RandomNumberGenerator.GetInt32((int)Math.Pow(10, length - 1), (int)Math.Pow(10, length));
        return random.ToString();
    }

    private static string MaskDestination(string destination)
    {
        var atIndex = destination.IndexOf('@');
        if (atIndex < 2)
            return "***";
        var local = destination[..atIndex];
        var domain = destination[atIndex..];
        return $"{local[0]}***{local[^1]}{domain}";
    }

    private static OtpChallengeResponseDto MapChallenge(OtpChallenge challenge)
    {
        var now = DateTime.UtcNow;
        return new OtpChallengeResponseDto
        {
            ChallengeId = challenge.ChallengeId,
            MaskedDestination = MaskDestination(challenge.Destination),
            ExpiresInSeconds = Math.Max(0, (int)(challenge.ExpiresAt - now).TotalSeconds),
            ResendInSeconds = Math.Max(0, (int)(challenge.ResendAvailableAt - now).TotalSeconds)
        };
    }

    private async Task<OtpChallenge> GetActiveChallengeAsync(Guid challengeId, OtpPurpose purpose)
    {
        var challenge = (await _otpRepository.FindAsync(c => c.ChallengeId == challengeId && c.Purpose == purpose && !c.IsDeleted))
            .FirstOrDefault();
        if (challenge == null || challenge.ConsumedAt.HasValue || challenge.ExpiresAt <= DateTime.UtcNow)
            throw new InvalidOperationException("OTP challenge is no longer active.");
        return challenge;
    }

    private async Task ValidateOtpAttemptAsync(OtpChallenge challenge, string otpCode)
    {
        if (string.IsNullOrWhiteSpace(otpCode))
            throw new ArgumentException("OTP code is required.");
        if (challenge.AttemptCount >= challenge.MaxAttempts)
            throw new InvalidOperationException("Maximum OTP attempts reached.");

        if (!VerifyPassword(otpCode, challenge.CodeHash))
        {
            challenge.AttemptCount++;
            challenge.UpdatedAt = DateTime.UtcNow;
            await _otpRepository.UpdateAsync(challenge);
            await _otpRepository.SaveChangesAsync();
            throw new UnauthorizedAccessException("Invalid OTP code.");
        }
    }

    private static void ValidateRegistration(RegisterUserDto registerDto)
    {
        var emailValidation = UserRules.Validations.ValidateEmail(registerDto.Email);
        if (!emailValidation.IsValid)
            throw new ArgumentException(emailValidation.Message);
        var firstNameValidation = UserRules.Validations.ValidateName(registerDto.FirstName);
        if (!firstNameValidation.IsValid)
            throw new ArgumentException("First name: " + firstNameValidation.Message);
        var lastNameValidation = UserRules.Validations.ValidateName(registerDto.LastName);
        if (!lastNameValidation.IsValid)
            throw new ArgumentException("Last name: " + lastNameValidation.Message);
        if (string.IsNullOrWhiteSpace(registerDto.PhoneNumber))
            throw new ArgumentException("Phone number is required.");
        if (!Enum.IsDefined(typeof(UserRole), registerDto.Role))
            throw new ArgumentException("Invalid role selected.");
    }
}
