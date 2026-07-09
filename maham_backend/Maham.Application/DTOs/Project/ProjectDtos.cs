using System;
using System.Collections.Generic;
using Maham.Application.DTOs.Auth;
using Maham.Domain.Enums;

namespace Maham.Application.DTOs.Project;

public class CreateProjectDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateProjectDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class ProjectResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid OwnerId { get; set; }
    public UserResponseDto Owner { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int BoardCount { get; set; }
    public int MemberCount { get; set; }
}

public class ProjectMemberResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public UserResponseDto User { get; set; } = null!;
    public ProjectMemberRole Role { get; set; }
    public ProjectMemberStatus Status { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class ProjectInvitationDto
{
    public Guid MemberId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? ProjectDescription { get; set; }
    public string InviterName { get; set; } = string.Empty;
    public ProjectMemberRole Role { get; set; }
    public DateTime InvitedAt { get; set; }
}

public class AddProjectMemberDto
{
    public string Email { get; set; } = null!;
    public ProjectMemberRole Role { get; set; } = ProjectMemberRole.Member;
}

public class UpdateProjectMemberDto
{
    public ProjectMemberRole Role { get; set; }
}
