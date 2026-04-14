using PawNect.Application.DTOs.User;

namespace PawNect.Application.Interfaces;

/// <summary>
/// User Service interface
/// </summary>
public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<IEnumerable<UserDto>> GetUsersByRoleAsync(int role);
    Task<OtpChallengeResponseDto> InitiateRegistrationAsync(RegisterUserDto registerDto, CancellationToken cancellationToken = default);
    Task<UserDto> VerifyRegistrationOtpAsync(RegisterVerifyOtpDto verifyDto, CancellationToken cancellationToken = default);
    Task<OtpChallengeResponseDto> InitiateLoginAsync(LoginUserDto loginDto, CancellationToken cancellationToken = default);
    Task<UserDto?> VerifyLoginOtpAsync(LoginVerifyOtpDto verifyDto, CancellationToken cancellationToken = default);
    Task<OtpChallengeResponseDto> ResendOtpAsync(ResendOtpDto resendDto, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserAsync(int userId, UserDto updateDto);
    Task<bool> DeleteUserAsync(int userId);
}
