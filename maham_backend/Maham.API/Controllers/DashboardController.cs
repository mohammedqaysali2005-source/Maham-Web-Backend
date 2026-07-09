using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maham.Application.DTOs.Common;
using Maham.Application.DTOs.Dashboard;
using Maham.Application.Services.Interfaces;

namespace Maham.API.Controllers;

[Authorize]
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetGeneralDashboard()
    {
        var result = await _dashboardService.GetGeneralDashboardAsync(CurrentUserId);
        return Ok(ApiResponseDto<DashboardStatsDto>.SuccessResponse(result));
    }

    [HttpGet("charts")]
    public async Task<IActionResult> GetDashboardCharts()
    {
        var result = await _dashboardService.GetDashboardChartsAsync(CurrentUserId);
        return Ok(ApiResponseDto<ChartDataDto>.SuccessResponse(result));
    }
}
