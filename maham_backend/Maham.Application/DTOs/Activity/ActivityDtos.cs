using System;
using Maham.Application.DTOs.Auth;

namespace Maham.Application.DTOs.Activity;

public class ActivityLogResponseDto
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public UserResponseDto User { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}
