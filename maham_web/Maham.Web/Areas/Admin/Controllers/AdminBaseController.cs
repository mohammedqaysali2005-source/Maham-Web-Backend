using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Maham.Web.Helpers;

namespace Maham.Web.Areas.Admin.Controllers;

[Area("Admin")]
public abstract class AdminBaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        base.OnActionExecuting(filterContext);

        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
        {
            filterContext.Result = new RedirectToActionResult("Login", "Auth", new { area = "" });
            return;
        }

        if (!SessionHelper.IsAdmin(HttpContext.Session))
        {
            TempData["Error"] = "غير مصرح لك بالوصول لمنطقة الإدارة (تطلب صلاحية Admin).";
            filterContext.Result = new RedirectToActionResult("Index", "Home", new { area = "" });
            return;
        }
    }
}
