using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PawNect.PetParent.Web.Infrastructure;
using PawNect.PetParent.Web.Models;
using PawNect.PetParent.Web.Services;

namespace PawNect.PetParent.Web.Controllers;

/// <summary>
/// Flow 3 – Preventive Lab Tests (Pet Parent–Initiated). A: Pet Parent Flow (Entry → Landing → Select Test → Select Pet & Collection → Review & Confirm).
/// B: Lab Flow — Preventive status. C: Report Flow (Report PDF, interpretation note, CTAs: Book Online Consult, Book In-Clinic Appointment).
/// </summary>
public class LabTestsController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public LabTestsController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// A1/A2. Entry CTA → Landing Page: Preventive Health Tests, Wellness Packages, Individual Tests
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        return View(new LabTestLandingViewModel());
    }

    /// <summary>
    /// A3. Select Test Type – Packages (Basic Health Check, Annual Wellness, Senior Panel) or Individual (CBC, LFT, KFT, Thyroid)
    /// </summary>
    [HttpGet]
    public IActionResult SelectTestType()
    {
        var packages = LabTestStore.GetPackages();
        var individual = LabTestStore.GetIndividualTests();
        return View(new LabTestSelectTypeViewModel
        {
            Packages = packages.ToList(),
            IndividualTests = individual.ToList()
        });
    }

    /// <summary>
    /// A4. Select Pet & Collection Type – pet selection, Home collection OR In-clinic. Login required.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SelectPetAndCollection(string testId, string testType)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (!userId.HasValue)
        {
            TempData["ErrorMessage"] = "Please sign in to book a lab test.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(SelectPetAndCollection), new { testId, testType }) });
        }
        if (!SessionHelper.IsParent(HttpContext.Session))
        {
            TempData["ErrorMessage"] = "This section is for pet parents.";
            return RedirectToAction("Index", "Home");
        }

        string displayName;
        decimal price;
        string sampleType = "Blood";

        if (testType == "package")
        {
            var pkg = LabTestStore.GetPackage(testId);
            if (pkg == null) return RedirectToAction(nameof(SelectTestType));
            displayName = pkg.Name;
            price = pkg.Price;
            sampleType = pkg.SampleType;
        }
        else
        {
            var ind = LabTestStore.GetIndividualTest(testId);
            if (ind == null) return RedirectToAction(nameof(SelectTestType));
            displayName = ind.FullName ?? ind.Name;
            price = ind.Price;
            sampleType = ind.SampleType;
        }

        var pets = await GetUserPetsAsync(userId.Value);
        if (!pets.Any())
        {
            TempData["ErrorMessage"] = "Please add a pet first to book a lab test.";
            return RedirectToAction("Index", "Pets");
        }

        return View(new LabOrderSelectPetViewModel
        {
            TestId = testId,
            TestType = testType,
            TestDisplayName = displayName,
            Price = price,
            SampleType = sampleType,
            Pets = pets,
            SelectedPetId = pets.First().Id,
            CollectionType = "Home"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectPetAndCollection(LabOrderSelectPetViewModel model)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (!userId.HasValue)
        {
            TempData["ErrorMessage"] = "Please sign in to continue.";
            return RedirectToAction("Login", "Account");
        }
        if (!SessionHelper.IsParent(HttpContext.Session))
        {
            TempData["ErrorMessage"] = "This section is for pet parents.";
            return RedirectToAction("Index", "Home");
        }

        model.Pets = await GetUserPetsAsync(userId.Value);
        var selectedPet = model.Pets.FirstOrDefault(p => p.Id == model.SelectedPetId);
        if (selectedPet == null)
        {
            ModelState.AddModelError("SelectedPetId", "Please select a pet.");
            return View(model);
        }

        return RedirectToAction(nameof(ReviewConfirm), new
        {
            testId = model.TestId,
            testType = model.TestType,
            petId = model.SelectedPetId,
            collectionType = model.CollectionType
        });
    }

    /// <summary>
    /// A5/A6. Review & Confirm – no lab selection UI (PawNect assigns internally). Placeholder payment, order confirmation.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ReviewConfirm(string testId, string testType, int petId, string collectionType)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (!userId.HasValue)
        {
            TempData["ErrorMessage"] = "Please sign in to continue.";
            return RedirectToAction("Login", "Account");
        }
        if (!SessionHelper.IsParent(HttpContext.Session))
        {
            TempData["ErrorMessage"] = "This section is for pet parents.";
            return RedirectToAction("Index", "Home");
        }

        string displayName;
        decimal price;
        string sampleType = "Blood";

        if (testType == "package")
        {
            var pkg = LabTestStore.GetPackage(testId);
            if (pkg == null) return RedirectToAction(nameof(SelectTestType));
            displayName = pkg.Name;
            price = pkg.Price;
            sampleType = pkg.SampleType;
        }
        else
        {
            var ind = LabTestStore.GetIndividualTest(testId);
            if (ind == null) return RedirectToAction(nameof(SelectTestType));
            displayName = ind.FullName ?? ind.Name;
            price = ind.Price;
            sampleType = ind.SampleType;
        }

        var pets = await GetUserPetsAsync(userId.Value);
        var pet = pets.FirstOrDefault(p => p.Id == petId);
        if (pet == null) return RedirectToAction(nameof(SelectTestType));

        return View(new LabOrderReviewViewModel
        {
            TestId = testId,
            TestType = testType,
            TestDisplayName = displayName,
            Price = price,
            SampleType = sampleType,
            PetName = pet.Name,
            PetId = petId,
            CollectionType = collectionType ?? "Home"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewConfirm(LabOrderReviewViewModel model)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (!userId.HasValue)
        {
            TempData["ErrorMessage"] = "Please sign in to confirm.";
            return RedirectToAction("Login", "Account");
        }
        if (!SessionHelper.IsParent(HttpContext.Session))
        {
            TempData["ErrorMessage"] = "This section is for pet parents.";
            return RedirectToAction("Index", "Home");
        }

        var orderId = LabTestStore.CreateOrder(
            userId.Value,
            model.TestId,
            model.TestType,
            model.TestDisplayName,
            model.Price,
            model.PetName,
            model.PetId,
            model.CollectionType);

        TempData["SuccessMessage"] = "Lab test order placed successfully. PawNect will assign a lab and contact you for sample collection.";
        return RedirectToAction(nameof(ConfirmSuccess), new { orderId });
    }

    /// <summary>
    /// A6. Order confirmation (post-place order)
    /// </summary>
    [HttpGet]
    public IActionResult ConfirmSuccess(string orderId)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (!userId.HasValue || !LabTestStore.UserOwnsOrder(userId.Value, orderId))
        {
            TempData["ErrorMessage"] = "Order not found.";
            return RedirectToAction(nameof(Index));
        }
        if (!SessionHelper.IsParent(HttpContext.Session))
        {
            TempData["ErrorMessage"] = "This section is for pet parents.";
            return RedirectToAction("Index", "Home");
        }
        var order = LabTestStore.GetOrder(orderId);
        if (order == null) return RedirectToAction(nameof(Index));
        return View(order);
    }

    /// <summary>
    /// B. Lab Flow — Preventive: list orders with status (Ordered, Sample Scheduled, Sample Collected, Processing, Report Uploaded)
    /// </summary>
    [HttpGet]
    public IActionResult OrderStatus()
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (!userId.HasValue)
        {
            TempData["ErrorMessage"] = "Please sign in to view your lab orders.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(OrderStatus)) });
        }
        if (!SessionHelper.IsParent(HttpContext.Session))
        {
            TempData["ErrorMessage"] = "This section is for pet parents.";
            return RedirectToAction("Index", "Home");
        }
        var orders = LabTestStore.GetOrdersByUser(userId.Value);
        return View(orders);
    }

    [HttpGet]
    public IActionResult OrderDetail(string id)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (!userId.HasValue || !LabTestStore.UserOwnsOrder(userId.Value, id))
        {
            TempData["ErrorMessage"] = "Order not found.";
            return RedirectToAction(nameof(OrderStatus));
        }
        if (!SessionHelper.IsParent(HttpContext.Session))
        {
            TempData["ErrorMessage"] = "This section is for pet parents.";
            return RedirectToAction("Index", "Home");
        }
        var order = LabTestStore.GetOrder(id);
        if (order == null) return RedirectToAction(nameof(OrderStatus));
        if (order.HasReport)
            return RedirectToAction(nameof(Report), new { orderId = id });
        return View(order);
    }

    /// <summary>
    /// C. Report Flow – Report PDF, interpretation note, CTAs: Book Online Consult, Book In-Clinic
    /// </summary>
    [HttpGet]
    public IActionResult Report(string orderId)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (!userId.HasValue || !LabTestStore.UserOwnsOrder(userId.Value, orderId))
        {
            TempData["ErrorMessage"] = "Report not found.";
            return RedirectToAction(nameof(OrderStatus));
        }
        if (!SessionHelper.IsParent(HttpContext.Session))
        {
            TempData["ErrorMessage"] = "This section is for pet parents.";
            return RedirectToAction("Index", "Home");
        }
        var report = LabTestStore.GetReport(orderId);
        if (report == null)
        {
            TempData["ErrorMessage"] = "Report not yet available.";
            return RedirectToAction(nameof(OrderDetail), new { id = orderId });
        }
        return View(report);
    }

    private async Task<List<PetOptionViewModel>> GetUserPetsAsync(int ownerId)
    {
        try
        {
            var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "http://localhost:5000/api";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{baseUrl}/pets/owner/{ownerId}");
            if (!response.IsSuccessStatusCode) return new List<PetOptionViewModel>();
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponseViewModel<List<LabTestPetApiDto>>>(content, JsonOptions);
            if (apiResponse?.Success != true || apiResponse.Data == null) return new List<PetOptionViewModel>();
            return apiResponse.Data.Select(p => new PetOptionViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Species = PetSpeciesHelper.GetSpeciesName(p.Species)
            }).ToList();
        }
        catch
        {
            return new List<PetOptionViewModel>();
        }
    }
}

internal class LabTestPetApiDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Species { get; set; }
}
