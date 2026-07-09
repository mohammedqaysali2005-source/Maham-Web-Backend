using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maham.Application.DTOs.Common;
using Maham.Application.DTOs.Auth;
using Maham.Application.Services.Interfaces;

namespace Maham.API.Controllers;

[Authorize]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return CreatedAtAction(nameof(GetMe), ApiResponseDto<AuthResponseDto>.SuccessResponse(result, "User registered successfully."));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(ApiResponseDto<AuthResponseDto>.SuccessResponse(result, "Login successful."));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var result = await _authService.GetMeAsync(CurrentUserId);
        return Ok(ApiResponseDto<UserResponseDto>.SuccessResponse(result));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var result = await _authService.UpdateProfileAsync(CurrentUserId, dto);
        return Ok(ApiResponseDto<UserResponseDto>.SuccessResponse(result, "Profile updated successfully."));
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        await _authService.ChangePasswordAsync(CurrentUserId, dto);
        return Ok(ApiResponseDto<object>.SuccessResponse(null, "Password changed successfully."));
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // JWT is stateless; client should delete token
        return Ok(ApiResponseDto<object>.SuccessResponse(null, "Logged out successfully."));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var result = await _authService.RefreshTokenAsync(CurrentUserId);
        return Ok(ApiResponseDto<AuthResponseDto>.SuccessResponse(result, "Token refreshed successfully."));
    }
}
