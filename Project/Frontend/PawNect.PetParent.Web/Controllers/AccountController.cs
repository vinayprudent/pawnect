using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PawNect.PetParent.Web.Models;

namespace PawNect.PetParent.Web.Controllers;

public class AccountController : Controller
{
    private const string UserIdKey = "UserId";
    private const string UserEmailKey = "UserEmail";
    private const string UserNameKey = "UserName";
    private const string UserRoleKey = "UserRole";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AccountController> _logger;
    private readonly string _apiBaseUrl;
    private readonly string? _apiFallbackBaseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public AccountController(IHttpClientFactory httpClientFactory, ILogger<AccountController> logger, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "http://localhost:5000/api";
        _apiFallbackBaseUrl = _apiBaseUrl.Contains("localhost:5000", StringComparison.OrdinalIgnoreCase)
            ? _apiBaseUrl.Replace("http://localhost:5000", "https://localhost:5001", StringComparison.OrdinalIgnoreCase)
            : null;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    private async Task<HttpResponseMessage> PostWithLocalFallbackAsync(HttpClient client, string relativePath, HttpContent content, CancellationToken cancellationToken)
    {
        try
        {
            return await client.PostAsync($"{_apiBaseUrl}{relativePath}", content, cancellationToken);
        }
        catch (HttpRequestException) when (!string.IsNullOrWhiteSpace(_apiFallbackBaseUrl))
        {
            // Local dev fallback when API is bound on https://localhost:5001 instead of http://localhost:5000.
            return await client.PostAsync($"{_apiFallbackBaseUrl}{relativePath}", content, cancellationToken);
        }
    }

    private T? TryDeserialize<T>(string? content) where T : class
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var client = _httpClientFactory.CreateClient();
            var body = new { emailOrMobile = model.EmailOrMobile };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await PostWithLocalFallbackAsync(client, "/users/login/initiate", content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = TryDeserialize<ApiResponseViewModel<object>>(responseBody);
                ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "Invalid email or mobile number.");
                return View(model);
            }

            var apiResponse = TryDeserialize<ApiResponseViewModel<OtpChallengeViewModel>>(responseBody);
            if (apiResponse?.Success != true || apiResponse.Data == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or mobile number.");
                return View(model);
            }

            var verifyModel = new VerifyOtpViewModel
            {
                ChallengeId = apiResponse.Data.ChallengeId,
                Purpose = "Login",
                MaskedDestination = apiResponse.Data.MaskedDestination,
                ReturnUrl = returnUrl
            };
            return View("VerifyOtp", verifyModel);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error during login");
            ModelState.AddModelError(string.Empty, "Cannot reach API at localhost:5000. Start PawNect API and try again.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            ModelState.AddModelError(string.Empty, "Unable to sign in. Please try again later.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var fullName = (model.FullName ?? string.Empty).Trim();
            var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
            var lastName = nameParts.Length > 1 ? string.Join(' ', nameParts[1..]) : firstName;

            var client = _httpClientFactory.CreateClient();
            var body = new
            {
                firstName = firstName,
                lastName = lastName,
                email = model.Email,
                phoneNumber = model.PhoneNumber,
                role = 1, // PetParent
                address = model.Address,
                city = model.City,
                state = model.State,
                zipCode = model.ZipCode
            };
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await PostWithLocalFallbackAsync(client, "/users/register/initiate", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var errorResponse = TryDeserialize<ApiResponseViewModel<object>>(errorContent);
                ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "Registration failed. Please try again.");
                return View(model);
            }

            var successContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = TryDeserialize<ApiResponseViewModel<OtpChallengeViewModel>>(successContent);
            if (apiResponse?.Success != true || apiResponse.Data == null)
            {
                ModelState.AddModelError(string.Empty, "Registration could not be initiated.");
                return View(model);
            }

            var verifyModel = new VerifyOtpViewModel
            {
                ChallengeId = apiResponse.Data.ChallengeId,
                Purpose = "Register",
                MaskedDestination = apiResponse.Data.MaskedDestination
            };
            return View("VerifyOtp", verifyModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            ModelState.AddModelError(string.Empty, "Unable to register. Please try again later.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var client = _httpClientFactory.CreateClient();
            var endpoint = model.Purpose.Equals("Register", StringComparison.OrdinalIgnoreCase)
                ? "/users/register/verify"
                : "/users/login/verify";
            var payload = new { challengeId = model.ChallengeId, otpCode = model.OtpCode };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await PostWithLocalFallbackAsync(client, endpoint, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var errorResponse = TryDeserialize<ApiResponseViewModel<object>>(errorContent);
                ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "OTP verification failed.");
                return View(model);
            }

            if (model.Purpose.Equals("Register", StringComparison.OrdinalIgnoreCase))
            {
                TempData["SuccessMessage"] = "Account verified successfully. Please sign in.";
                return RedirectToAction(nameof(Login));
            }

            var successContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = TryDeserialize<ApiResponseViewModel<UserSessionDto>>(successContent);
            if (apiResponse?.Success != true || apiResponse.Data == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to complete login.");
                return View(model);
            }

            var user = apiResponse.Data;

            HttpContext.Session.SetInt32(UserIdKey, user.Id);
            HttpContext.Session.SetString(UserEmailKey, user.Email);
            HttpContext.Session.SetString(UserNameKey, $"{user.FirstName} {user.LastName}".Trim());
            HttpContext.Session.SetInt32(UserRoleKey, user.Role);
            TempData["SuccessMessage"] = "Welcome back!";
            return LocalRedirect(model.ReturnUrl ?? Url.Action("Index", "Home") ?? "/");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP");
            ModelState.AddModelError(string.Empty, "Unable to verify OTP. Please try again.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp(VerifyOtpViewModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var payload = new { challengeId = model.ChallengeId };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await PostWithLocalFallbackAsync(client, "/users/otp/resend", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var errorResponse = TryDeserialize<ApiResponseViewModel<object>>(errorContent);
                ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "Unable to resend OTP.");
                return View("VerifyOtp", model);
            }

            var successContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = TryDeserialize<ApiResponseViewModel<OtpChallengeViewModel>>(successContent);
            model.MaskedDestination = apiResponse?.Data?.MaskedDestination ?? model.MaskedDestination;
            TempData["SuccessMessage"] = "OTP resent successfully.";
            return View("VerifyOtp", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending OTP");
            ModelState.AddModelError(string.Empty, "Unable to resend OTP right now.");
            return View("VerifyOtp", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["SuccessMessage"] = "You have been signed out.";
        return RedirectToAction("Index", "Home");
    }
}
