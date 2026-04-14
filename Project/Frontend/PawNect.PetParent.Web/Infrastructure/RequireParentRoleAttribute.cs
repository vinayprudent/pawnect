using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PawNect.PetParent.Web.Infrastructure;

/// <summary>
/// Restricts access to Pet Parent only. Vet and Lab users are redirected to Home with a message.
/// Apply to controllers/actions that are for pet parents (e.g. Pets, Appointments, Consultations).
/// </summary>
public class RequireParentRoleAttribute : Attribute, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        if (!SessionHelper.IsAuthenticated(session))
            return; // Let the action handle redirect to Login

        if (SessionHelper.IsParent(session))
            return;

        if (context.Controller is Controller controller)
            controller.TempData["ErrorMessage"] = "This section is for pet parents. Please sign in as Pet Parent.";
        context.Result = new RedirectToActionResult("Index", "Home", null);
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
