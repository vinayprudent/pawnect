using PawNect.Application.DTOs;
using PawNect.Application.DTOs.User;

namespace PawNect.AdminPortal.Web.Services;

public interface IApiClient
{
    Task<ApiResponse<UserDto>?> LoginAsync(string email, string password);
    Task<ApiResponse<IEnumerable<UserDto>>?> GetUsersByRoleAsync(int role);
    Task<ApiResponse<UserDto>?> GetUserByIdAsync(int id);
    Task<ApiResponse<UserDto>?> RegisterUserAsync(RegisterUserDto dto);
    Task<ApiResponse<UserDto>?> UpdateUserAsync(int id, UserDto dto);
    Task<ApiResponse?> DeleteUserAsync(int id);
}
