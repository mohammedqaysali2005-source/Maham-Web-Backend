using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Maham.Web.Services;
using Maham.Web.Helpers;

namespace Maham.Web.Controllers;

public class ActivityController : Controller
{
    private readonly ApiService _api;

    public ActivityController(ApiService api)
    {
        _api = api;
    }

    [HttpGet("/activity")]
    public async Task<IActionResult> Index()
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<List<WebActivityDto>>("/api/activity/me");
        return View(data ?? new List<WebActivityDto>());
    }

    [HttpGet("/activity/me")]
    public async Task<IActionResult> Me()
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<List<WebActivityDto>>("/api/activity/me");
        return View("Index", data ?? new List<WebActivityDto>());
    }
}
