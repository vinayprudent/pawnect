using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using PawNect.Application.DTOs;
using PawNect.Application.DTOs.User;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PawNect.AdminPortal.Web.Services;

namespace PawNect.AdminPortal.Web.Controllers;

public class AccountController : Controller
{
    private const int AdminRole = 5; // UserRole.Admin
    private readonly IApiClient _api;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IApiClient api, ILogger<AccountController> logger)
    {
        _api = api;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToLocal(returnUrl);
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl, CancellationToken cancellationToken)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Email and password are required.");
            return View();
        }

        ApiResponse<UserDto>? response;
        try
        {
            response = await _api.LoginAsync(email.Trim(), password);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Cannot reach the API. Ensure PawNect API is running at the configured URL (e.g. http://localhost:5000).");
            return View();
        }
        catch (TaskCanceledException)
        {
            ModelState.AddModelError("", "Request timed out. Check that the API is running and reachable.");
            return View();
        }

        if (response?.Success != true || response.Data == null)
        {
            ModelState.AddModelError("", response?.Message ?? "Invalid email or password.");
            return View();
        }

        if (response.Data.Role != AdminRole)
        {
            ModelState.AddModelError("", "Access denied. Admin credentials required.");
            return View();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, response.Data.Email),
            new(ClaimTypes.Name, $"{response.Data.FirstName} {response.Data.LastName}".Trim()),
            new(ClaimTypes.NameIdentifier, response.Data.Id.ToString()),
            new(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true });
        _logger.LogInformation("Admin logged in: {Email}", response.Data.Email);
        return RedirectToLocal(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            // When app is under a path base (e.g. /admin), ensure redirect stays under it
            var pathBase = HttpContext.Request.PathBase.Value ?? "";
            if (!string.IsNullOrEmpty(pathBase))
                return Redirect(pathBase + (returnUrl.StartsWith("/") ? returnUrl : "/" + returnUrl));
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Dashboard");
    }
}
