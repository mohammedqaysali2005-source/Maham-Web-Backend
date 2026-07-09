using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maham.Application.DTOs.Common;
using Maham.Application.DTOs.Project;
using Maham.Application.DTOs.Board;
using Maham.Application.DTOs.Card;
using Maham.Application.DTOs.Activity;
using Maham.Application.DTOs.Dashboard;
using Maham.Application.Services.Interfaces;

namespace Maham.API.Controllers;

[Authorize]
public class ProjectsController : BaseApiController
{
    private readonly IProjectService _projectService;
    private readonly IBoardService _boardService;
    private readonly ICardService _cardService;
    private readonly IActivityService _activityService;
    private readonly IDashboardService _dashboardService;

    public ProjectsController(
        IProjectService projectService,
        IBoardService boardService,
        ICardService cardService,
        IActivityService activityService,
        IDashboardService dashboardService)
    {
        _projectService = projectService;
        _boardService = boardService;
        _cardService = cardService;
        _activityService = activityService;
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserProjects()
    {
        var result = await _projectService.GetUserProjectsAsync(CurrentUserId);
        return Ok(ApiResponseDto<IEnumerable<ProjectResponseDto>>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        var result = await _projectService.CreateProjectAsync(CurrentUserId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponseDto<ProjectResponseDto>.SuccessResponse(result, "Project created successfully."));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _projectService.GetProjectByIdAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<ProjectResponseDto>.SuccessResponse(result));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDto dto)
    {
        var result = await _projectService.UpdateProjectAsync(id, CurrentUserId, dto);
        return Ok(ApiResponseDto<ProjectResponseDto>.SuccessResponse(result, "Project updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _projectService.DeleteProjectAsync(id, CurrentUserId);
        return NoContent();
    }

    [HttpGet("{id}/members")]
    public async Task<IActionResult> GetMembers(Guid id)
    {
        var result = await _projectService.GetProjectMembersAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<IEnumerable<ProjectMemberResponseDto>>.SuccessResponse(result));
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddProjectMemberDto dto)
    {
        var result = await _projectService.AddProjectMemberAsync(id, CurrentUserId, dto);
        return Ok(ApiResponseDto<ProjectMemberResponseDto>.SuccessResponse(result, "Member added successfully."));
    }

    [HttpPut("{id}/members/{uid}")]
    public async Task<IActionResult> UpdateMemberRole(Guid id, Guid uid, [FromBody] UpdateProjectMemberDto dto)
    {
        var result = await _projectService.UpdateProjectMemberAsync(id, CurrentUserId, uid, dto);
        return Ok(ApiResponseDto<ProjectMemberResponseDto>.SuccessResponse(result, "Member role updated successfully."));
    }

    [HttpDelete("{id}/members/{uid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid uid)
    {
        await _projectService.RemoveProjectMemberAsync(id, CurrentUserId, uid);
        return NoContent();
    }

    [HttpGet("{id}/boards")]
    public async Task<IActionResult> GetBoards(Guid id)
    {
        var result = await _boardService.GetProjectBoardsAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<IEnumerable<BoardResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}/activity")]
    public async Task<IActionResult> GetActivity(Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _activityService.GetProjectActivityAsync(id, CurrentUserId, pageNumber, pageSize);
        return Ok(ApiResponseDto<PagedResponseDto<ActivityLogResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}/dashboard")]
    public async Task<IActionResult> GetDashboard(Guid id)
    {
        var result = await _dashboardService.GetProjectDashboardAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<DashboardStatsDto>.SuccessResponse(result));
    }

    [HttpGet("{id}/cards")]
    public async Task<IActionResult> GetCards(Guid id)
    {
        var result = await _cardService.GetProjectCardsAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<IEnumerable<CardResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}/overdue")]
    public async Task<IActionResult> GetOverdueCards(Guid id)
    {
        var result = await _cardService.GetProjectOverdueCardsAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<IEnumerable<CardResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("invitations/pending")]
    public async Task<IActionResult> GetPendingInvitations()
    {
        var result = await _projectService.GetPendingInvitationsAsync(CurrentUserId);
        return Ok(ApiResponseDto<IEnumerable<ProjectInvitationDto>>.SuccessResponse(result));
    }

    [HttpPost("invitations/{memberId}/accept")]
    public async Task<IActionResult> AcceptInvitation(Guid memberId)
    {
        await _projectService.AcceptInvitationAsync(memberId, CurrentUserId);
        return Ok(ApiResponseDto<object>.SuccessResponse(null, "Invitation accepted successfully."));
    }

    [HttpPost("invitations/{memberId}/reject")]
    public async Task<IActionResult> RejectInvitation(Guid memberId)
    {
        await _projectService.RejectInvitationAsync(memberId, CurrentUserId);
        return Ok(ApiResponseDto<object>.SuccessResponse(null, "Invitation rejected."));
    }
}
