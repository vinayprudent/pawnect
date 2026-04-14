using Microsoft.AspNetCore.Mvc;
using PawNect.Application.DTOs;
using PawNect.Application.DTOs.User;
using PawNect.Application.Interfaces;

namespace PawNect.API.Controllers;

/// <summary>
/// User API Controller for managing users and authentication
/// </summary>
[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResponse(users, "Users retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, ApiResponse<IEnumerable<UserDto>>.ErrorResponse("An error occurred while retrieving users"));
        }
    }

    /// <summary>
    /// Get users by role (e.g. 1=PetParent, 2=VeterinaryClinic, 6=Laboratory, 5=Admin)
    /// </summary>
    [HttpGet("byrole/{role}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetUsersByRole(int role)
    {
        try
        {
            var users = await _userService.GetUsersByRoleAsync(role);
            return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResponse(users, "Users retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users by role");
            return StatusCode(500, ApiResponse<IEnumerable<UserDto>>.ErrorResponse("An error occurred while retrieving users"));
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found"));

            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, ApiResponse<UserDto>.ErrorResponse("An error occurred while retrieving the user"));
        }
    }

    /// <summary>
    /// Initiate registration by sending OTP
    /// </summary>
    [HttpPost("register/initiate")]
    public async Task<ActionResult<ApiResponse<OtpChallengeResponseDto>>> RegisterInitiate([FromBody] RegisterUserDto registerDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<OtpChallengeResponseDto>.ErrorResponse("Invalid user data"));

            var challenge = await _userService.InitiateRegistrationAsync(registerDto, cancellationToken);
            return Ok(ApiResponse<OtpChallengeResponseDto>.SuccessResponse(challenge, "OTP sent successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error during registration");
            return BadRequest(ApiResponse<OtpChallengeResponseDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business logic error during registration");
            return BadRequest(ApiResponse<OtpChallengeResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user");
            return StatusCode(500, ApiResponse<OtpChallengeResponseDto>.ErrorResponse("An error occurred during registration"));
        }
    }

    /// <summary>
    /// Verify registration OTP and create account.
    /// </summary>
    [HttpPost("register/verify")]
    public async Task<ActionResult<ApiResponse<UserDto>>> RegisterVerify([FromBody] RegisterVerifyOtpDto verifyDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<UserDto>.ErrorResponse("Invalid OTP request"));

            var user = await _userService.VerifyRegistrationOtpAsync(verifyDto, cancellationToken);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, ApiResponse<UserDto>.SuccessResponse(user, "User registered successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Invalid OTP during registration verification");
            return Unauthorized(ApiResponse<UserDto>.ErrorResponse("Invalid OTP code"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid registration OTP challenge");
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(ex.Message));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error during registration OTP verification");
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying registration OTP");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResponse("An error occurred during registration verification"));
        }
    }

    /// <summary>
    /// Initiate login by validating email/mobile and sending OTP.
    /// </summary>
    [HttpPost("login/initiate")]
    public async Task<ActionResult<ApiResponse<OtpChallengeResponseDto>>> LoginInitiate([FromBody] LoginUserDto loginDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<OtpChallengeResponseDto>.ErrorResponse("Invalid request"));

            var challenge = await _userService.InitiateLoginAsync(loginDto, cancellationToken);
            return Ok(ApiResponse<OtpChallengeResponseDto>.SuccessResponse(challenge, "OTP sent successfully"));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(ApiResponse<OtpChallengeResponseDto>.ErrorResponse("Invalid email or mobile number"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login initiate");
            return StatusCode(500, ApiResponse<OtpChallengeResponseDto>.ErrorResponse("An error occurred during login"));
        }
    }

    /// <summary>
    /// Verify login OTP. Returns user data on success.
    /// </summary>
    [HttpPost("login/verify")]
    public async Task<ActionResult<ApiResponse<UserDto>>> LoginVerify([FromBody] LoginVerifyOtpDto verifyDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<UserDto>.ErrorResponse("Invalid OTP request"));

            var user = await _userService.VerifyLoginOtpAsync(verifyDto, cancellationToken);
            if (user == null)
                return Unauthorized(ApiResponse<UserDto>.ErrorResponse("Unable to complete login"));
            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User logged in successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Invalid OTP during login verification");
            return Unauthorized(ApiResponse<UserDto>.ErrorResponse("Invalid OTP code"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid login OTP challenge");
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login verification");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResponse("An error occurred during login"));
        }
    }

    [HttpPost("otp/resend")]
    public async Task<ActionResult<ApiResponse<OtpChallengeResponseDto>>> ResendOtp([FromBody] ResendOtpDto resendDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var challenge = await _userService.ResendOtpAsync(resendDto, cancellationToken);
            return Ok(ApiResponse<OtpChallengeResponseDto>.SuccessResponse(challenge, "OTP resent successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<OtpChallengeResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while resending OTP");
            return StatusCode(500, ApiResponse<OtpChallengeResponseDto>.ErrorResponse("Unable to resend OTP"));
        }
    }

    /// <summary>
    /// Update user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(int id, [FromBody] UserDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<UserDto>.ErrorResponse("Invalid user data"));

            var success = await _userService.UpdateUserAsync(id, updateDto);
            if (!success)
                return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found"));

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found after update"));
            
            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, ApiResponse<UserDto>.ErrorResponse("An error occurred while updating the user"));
        }
    }

    /// <summary>
    /// Delete user
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteUser(int id)
    {
        try
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
                return NotFound(ApiResponse.ErrorResponse("User not found"));

            return Ok(ApiResponse.SuccessResponse("User deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting the user"));
        }
    }
}
