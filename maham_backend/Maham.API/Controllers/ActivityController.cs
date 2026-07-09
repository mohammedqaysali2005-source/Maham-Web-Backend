using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maham.Application.DTOs.Common;
using Maham.Application.DTOs.Activity;
using Maham.Application.Services.Interfaces;

namespace Maham.API.Controllers;

[Authorize]
public class ActivityController : BaseApiController
{
    private readonly IActivityService _activityService;

    public ActivityController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    /// <summary>
    /// GET /api/activity/me — سجل نشاطات المستخدم الحالي
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyActivity()
    {
        var result = await _activityService.GetUserActivityAsync(CurrentUserId);
        return Ok(ApiResponseDto<IEnumerable<ActivityLogResponseDto>>.SuccessResponse(result));
    }
}
