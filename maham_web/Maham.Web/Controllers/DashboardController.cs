using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Maham.Web.Services;
using Maham.Web.Helpers;

namespace Maham.Web.Controllers;

public class DashboardController : Controller
{
    private readonly ApiService _api;

    public DashboardController(ApiService api)
    {
        _api = api;
    }

    [HttpGet("/dashboard")]
    public async Task<IActionResult> Index()
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (okStats, stats, _) = await _api.GetAsync<WebDashboardDto>("/api/dashboard");
        var (okCharts, charts, _) = await _api.GetAsync<WebChartDataDto>("/api/dashboard/charts");
        var (okMyCards, myCards, _) = await _api.GetAsync<List<WebCardDto>>("/api/cards/assigned-to-me");
        var (okDueToday, dueToday, _) = await _api.GetAsync<List<WebCardDto>>("/api/cards/due-today");
        var (okAct, activities, _) = await _api.GetAsync<List<WebActivityDto>>("/api/activity/me");

        ViewBag.ChartsData = charts ?? new WebChartDataDto();
        ViewBag.MyCards = myCards ?? new List<WebCardDto>();
        ViewBag.DueToday = dueToday ?? new List<WebCardDto>();
        ViewBag.Activities = activities ?? new List<WebActivityDto>();

        return View(stats ?? new WebDashboardDto());
    }
}
