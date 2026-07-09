using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Maham.Application.DTOs.Common;
using Maham.Application.DTOs.Auth;
using Maham.Application.Services.Interfaces;

namespace Maham.API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userService.GetAllUsersAsync(CurrentUserId);
        return Ok(ApiResponseDto<IEnumerable<UserResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return Ok(ApiResponseDto<UserResponseDto>.SuccessResponse(result));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var result = await _userService.SearchUsersAsync(q);
        return Ok(ApiResponseDto<IEnumerable<UserResponseDto>>.SuccessResponse(result));
    }

    [HttpPost("{id}/avatar")]
    public async Task<IActionResult> UploadAvatar(Guid id, IFormFile file)
    {
        if (id != CurrentUserId)
        {
            throw new UnauthorizedAccessException("You can only upload your own avatar.");
        }
        var result = await _userService.UploadAvatarAsync(id, file);
        return Ok(ApiResponseDto<UserResponseDto>.SuccessResponse(result, "Avatar uploaded successfully."));
    }

    [HttpPut("{id}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleDto dto)
    {
        var result = await _userService.UpdateUserRoleAsync(id, dto.Role, CurrentUserId);
        return Ok(ApiResponseDto<UserResponseDto>.SuccessResponse(result, "Role updated successfully."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _userService.DeleteUserAsync(id, CurrentUserId);
        return NoContent();
    }
}
