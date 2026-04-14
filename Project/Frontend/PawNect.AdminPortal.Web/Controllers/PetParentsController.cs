using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawNect.Application.DTOs.User;
using PawNect.AdminPortal.Web.Models;
using PawNect.AdminPortal.Web.Services;

namespace PawNect.AdminPortal.Web.Controllers;

[Authorize]
public class PetParentsController : Controller
{
    private const int PetParentRole = 1; // UserRole.PetParent
    private readonly IApiClient _api;

    public PetParentsController(IApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try
        {
            var res = await _api.GetUsersByRoleAsync(PetParentRole);
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
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var res = await _api.GetUserByIdAsync(id);
        if (res?.Success != true || res.Data == null || res.Data.Role != PetParentRole)
            return NotFound();
        return View(res.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var res = await _api.GetUserByIdAsync(id);
        if (res?.Success != true || res.Data == null || res.Data.Role != PetParentRole)
            return NotFound();
        var vm = new EditPetParentViewModel
        {
            Id = res.Data.Id,
            FirstName = res.Data.FirstName,
            LastName = res.Data.LastName,
            Email = res.Data.Email,
            PhoneNumber = res.Data.PhoneNumber,
            Address = res.Data.Address,
            City = res.Data.City,
            State = res.Data.State,
            ZipCode = res.Data.ZipCode
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditPetParentViewModel model, CancellationToken ct)
    {
        if (id != model.Id) return NotFound();
        if (ModelState.IsValid)
        {
            var dto = new UserDto
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber ?? "",
                Role = PetParentRole,
                Address = model.Address,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode
            };
            var res = await _api.UpdateUserAsync(id, dto);
            if (res?.Success == true)
            {
                TempData["Success"] = "Pet parent updated.";
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
            TempData["Success"] = "Pet parent removed.";
        else
            TempData["Error"] = res?.Message ?? "Delete failed.";
        return RedirectToAction(nameof(Index));
    }
}
