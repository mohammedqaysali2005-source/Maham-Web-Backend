using System;

namespace Maham.Web.DTOs;

public class WebUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? AvatarUrl { get; set; }
}

public class WebAuthResponseDto
{
    public string Token { get; set; } = null!;
    public WebUserDto User { get; set; } = null!;
}
