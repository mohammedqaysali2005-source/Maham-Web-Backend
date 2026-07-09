using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Maham.Web.Services;

namespace Maham.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApiService _api;

    public HomeController(ApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var (ok, stats, _) = await _api.GetAsync<WebPublicStatsDto>("/api/stats/public");
        return View(stats ?? new WebPublicStatsDto());
    }

    [Route("/Home/Error")]
    public IActionResult Error()
    {
        return View("~/Views/Shared/Error.cshtml");
    }
}
