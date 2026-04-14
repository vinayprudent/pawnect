using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawNect.Application.DTOs.User;
using PawNect.AdminPortal.Web.Models;
using PawNect.AdminPortal.Web.Services;

namespace PawNect.AdminPortal.Web.Controllers;

[Authorize]
public class VetsController : Controller
{
    private const int VetRole = 2; // UserRole.VeterinaryClinic
    private readonly IApiClient _api;
    private readonly ILogger<VetsController> _logger;

    public VetsController(IApiClient api, ILogger<VetsController> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try
        {
            var res = await _api.GetUsersByRoleAsync(VetRole);
            var list = res?.Success == true ? res.Data ?? new List<UserDto>() : new List<UserDto>();
            if (res?.Success == false)
                TempData["Error"] = res.Message;
            return View(list);
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Cannot reach the API. Ensure the API is running (e.g. http://localhost:5000).";
            return View(new List<UserDto>());
        }
        catch (TaskCanceledException)
        {
            TempData["Error"] = "Request timed out. Check that the API is running.";
            return View(new List<UserDto>());
        }
    }

    [HttpGet]
    public IActionResult Create() => View(new RegisterVetViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegisterVetViewModel model, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            var dto = new RegisterUserDto
            {
                FirstName = model.ContactFirstName,
                LastName = model.ContactLastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber ?? "",
                Role = VetRole,
                Address = model.Address,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
                OrganizationName = model.ClinicName
            };
            try
            {
                var res = await _api.RegisterUserAsync(dto);
                if (res?.Success == true)
                {
                    TempData["Success"] = "Veterinary clinic registered successfully.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", res?.Message ?? "Registration failed.");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError("", "Cannot reach the API. Ensure the API is running (e.g. http://localhost:5000).");
            }
            catch (TaskCanceledException)
            {
                ModelState.AddModelError("", "Request timed out. Check that the API is running.");
            }
        }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var res = await _api.GetUserByIdAsync(id);
        if (res?.Success != true || res.Data == null || res.Data.Role != VetRole)
            return NotFound();
        var vm = new EditVetViewModel
        {
            Id = res.Data.Id,
            ContactFirstName = res.Data.FirstName,
            ContactLastName = res.Data.LastName,
            Email = res.Data.Email,
            PhoneNumber = res.Data.PhoneNumber,
            ClinicName = res.Data.OrganizationName,
            Address = res.Data.Address,
            City = res.Data.City,
            State = res.Data.State,
            ZipCode = res.Data.ZipCode
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditVetViewModel model, CancellationToken ct)
    {
        if (id != model.Id) return NotFound();
        if (ModelState.IsValid)
        {
            var dto = new UserDto
            {
                Id = model.Id,
                FirstName = model.ContactFirstName,
                LastName = model.ContactLastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber ?? "",
                Role = VetRole,
                OrganizationName = model.ClinicName,
                Address = model.Address,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode
            };
            var res = await _api.UpdateUserAsync(id, dto);
            if (res?.Success == true)
            {
                TempData["Success"] = "Veterinary clinic updated.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", res?.Message ?? "Update failed.");
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var res = await _api.DeleteUserAsync(id);
        if (res?.Success == true)
            TempData["Success"] = "Veterinary clinic removed.";
        else
            TempData["Error"] = res?.Message ?? "Delete failed.";
        return RedirectToAction(nameof(Index));
    }
}
