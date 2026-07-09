using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maham.Application.DTOs.Common;
using Maham.Application.DTOs.Dashboard;
using Maham.Application.Services.Interfaces;

namespace Maham.API.Controllers;

[ApiController]
[Route("api/stats")]
public class StatsController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public StatsController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicStats()
    {
        var result = await _dashboardService.GetPublicStatsAsync();
        return Ok(ApiResponseDto<PublicStatsDto>.SuccessResponse(result));
    }
}
