using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Maham.Web.Services;

namespace Maham.Web.Areas.Admin.Controllers;

public class DashboardController : AdminBaseController
{
    private readonly ApiService _api;

    public DashboardController(ApiService api)
    {
        _api = api;
    }

    [HttpGet("/admin")]
    [HttpGet("/admin/dashboard")]
    public async Task<IActionResult> Index()
    {
        var (okStats, stats, _) = await _api.GetAsync<WebDashboardDto>("/api/dashboard");
        var (okUsers, users, _) = await _api.GetAsync<List<WebUserDto>>("/api/users");
        var (okProjects, projects, _) = await _api.GetAsync<List<WebProjectDto>>("/api/projects");

        ViewBag.Stats = stats ?? new WebDashboardDto();
        ViewBag.UsersCount = users?.Count ?? 0;
        ViewBag.ProjectsCount = projects?.Count ?? 0;
        ViewBag.RecentUsers = users ?? new List<WebUserDto>();
        ViewBag.RecentProjects = projects ?? new List<WebProjectDto>();

        return View();
    }
}
