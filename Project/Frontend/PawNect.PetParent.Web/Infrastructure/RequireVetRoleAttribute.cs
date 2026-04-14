using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PawNect.PetParent.Web.Infrastructure;

/// <summary>
/// Restricts access to Vet (VeterinaryClinic) only. Parent and Lab users are redirected to Home.
/// Apply to controllers/actions for the vet portal (consultations, diagnostics, reports, guidance).
/// </summary>
public class RequireVetRoleAttribute : Attribute, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        if (!SessionHelper.IsAuthenticated(session))
        {
            if (context.Controller is Controller c)
                c.TempData["ErrorMessage"] = "Please sign in to access the vet portal.";
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
            return;
        }

        if (SessionHelper.IsVet(session))
            return;

        if (context.Controller is Controller controller)
            controller.TempData["ErrorMessage"] = "This section is for veterinary staff. Please sign in as Vet.";
        context.Result = new RedirectToActionResult("Index", "Home", null);
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
