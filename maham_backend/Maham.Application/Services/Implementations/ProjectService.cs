using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Maham.Domain.Entities;
using Maham.Domain.Interfaces;
using Maham.Domain.Enums;
using Maham.Application.DTOs.Project;
using Maham.Application.Services.Interfaces;

namespace Maham.Application.Services.Implementations;

public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IActivityService _activityService;
    private readonly ISignalRService _signalRService;

    public ProjectService(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        IActivityService activityService,
        ISignalRService signalRService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _activityService = activityService;
        _signalRService = signalRService;
    }

    private async Task ValidateProjectAccessAsync(Guid projectId, Guid userId)
    {
        var currentUser = await _unitOfWork.Users.GetByIdAsync(userId);
        if (currentUser?.Role == UserRole.Admin) return;

        var isOwner = await _unitOfWork.Projects.Query()
            .AnyAsync(p => p.Id == projectId && p.OwnerId == userId);
        
        var isMember = await _unitOfWork.ProjectMembers.Query()
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);

        if (!isOwner && !isMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this project.");
        }
    }

    private async Task ValidateProjectAdminAsync(Guid projectId, Guid userId)
    {
        var currentUser = await _unitOfWork.Users.GetByIdAsync(userId);
        if (currentUser?.Role == UserRole.Admin) return;

        var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new KeyNotFoundException("Project not found.");
        }

        if (project.OwnerId == userId) return;

        var membership = await _unitOfWork.ProjectMembers.Query()
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);

        if (membership == null || (membership.Role != ProjectMemberRole.Admin && membership.Role != ProjectMemberRole.Owner))
        {
            throw new UnauthorizedAccessException("Only project owners or administrators can perform this action.");
        }
    }

    public async Task<IEnumerable<ProjectResponseDto>> GetUserProjectsAsync(Guid userId)
    {
        var currentUser = await _unitOfWork.Users.GetByIdAsync(userId);
        var isAdmin = currentUser?.Role == UserRole.Admin;

        var query = _unitOfWork.Projects.Query()
            .Include(p => p.Owner)
            .Include(p => p.Boards)
            .Include(p => p.Members);

        var projects = isAdmin
            ? await query.OrderByDescending(p => p.CreatedAt).ToListAsync()
            : await query.Where(p => p.OwnerId == userId || p.Members.Any(m => m.UserId == userId && m.Status == ProjectMemberStatus.Accepted))
                .OrderByDescending(p => p.CreatedAt).ToListAsync();

        var dtos = _mapper.Map<List<ProjectResponseDto>>(projects);
        for (int i = 0; i < projects.Count; i++)
        {
            dtos[i].BoardCount = projects[i].Boards?.Count ?? 0;
            dtos[i].MemberCount = projects[i].Members?.Count(m => m.Status == ProjectMemberStatus.Accepted) ?? 0;
        }
        return dtos;
    }

    public async Task<ProjectResponseDto> CreateProjectAsync(Guid userId, CreateProjectDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            OwnerId = userId
        };

        await _unitOfWork.Projects.AddAsync(project);
        await _unitOfWork.SaveChangesAsync();

        // Add owner as a project member
        var member = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = userId,
            Role = ProjectMemberRole.Owner,
            Status = ProjectMemberStatus.Accepted
        };
        await _unitOfWork.ProjectMembers.AddAsync(member);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            project.Id,
            userId,
            "Project",
            project.Id,
            "ProjectCreated",
            $"Created the project '{project.Name}'"
        );

        var response = _mapper.Map<ProjectResponseDto>(project);
        response.Owner = _mapper.Map<DTOs.Auth.UserResponseDto>(user);
        return response;
    }

    public async Task<ProjectResponseDto> GetProjectByIdAsync(Guid projectId, Guid userId)
    {
        await ValidateProjectAccessAsync(projectId, userId);

        var project = await _unitOfWork.Projects.Query()
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            throw new KeyNotFoundException("Project not found.");
        }

        return _mapper.Map<ProjectResponseDto>(project);
    }

    public async Task<ProjectResponseDto> UpdateProjectAsync(Guid projectId, Guid userId, UpdateProjectDto dto)
    {
        await ValidateProjectAdminAsync(projectId, userId);

        var project = await _unitOfWork.Projects.Query()
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            throw new KeyNotFoundException("Project not found.");
        }

        project.Name = dto.Name;
        project.Description = dto.Description;
        project.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            projectId,
            userId,
            "Project",
            projectId,
            "ProjectUpdated",
            $"Updated the project details for '{project.Name}'"
        );

        return _mapper.Map<ProjectResponseDto>(project);
    }

    public async Task DeleteProjectAsync(Guid projectId, Guid userId)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new KeyNotFoundException("Project not found.");
        }

        var currentUser = await _unitOfWork.Users.GetByIdAsync(userId);
        var isGlobalAdmin = currentUser?.Role == UserRole.Admin;
        var isOwner = project.OwnerId == userId;

        if (!isOwner && !isGlobalAdmin)
        {
            throw new UnauthorizedAccessException("Only the project owner or a system administrator can delete a project.");
        }

        _unitOfWork.Projects.Delete(project);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProjectMemberResponseDto>> GetProjectMembersAsync(Guid projectId, Guid userId)
    {
        await ValidateProjectAccessAsync(projectId, userId);

        var members = await _unitOfWork.ProjectMembers.Query()
            .Include(m => m.User)
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.JoinedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ProjectMemberResponseDto>>(members);
    }

    public async Task<ProjectMemberResponseDto> AddProjectMemberAsync(Guid projectId, Guid userId, AddProjectMemberDto dto)
    {
        await ValidateProjectAdminAsync(projectId, userId);

        var userToAdd = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower().Trim());

        if (userToAdd == null)
        {
            throw new KeyNotFoundException($"No user found with the email '{dto.Email}'.");
        }

        var isAlreadyMember = await _unitOfWork.ProjectMembers.Query()
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userToAdd.Id);

        if (isAlreadyMember)
        {
            throw new ArgumentException("User is already a member of this project.");
        }

        var newMember = new ProjectMember
        {
            ProjectId = projectId,
            UserId = userToAdd.Id,
            Role = dto.Role,
            Status = ProjectMemberStatus.Pending
        };

        await _unitOfWork.ProjectMembers.AddAsync(newMember);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            projectId,
            userId,
            "Member",
            newMember.Id,
            "MemberJoined",
            $"Added '{userToAdd.FullName}' to the project members as '{dto.Role}'"
        );

        // Notify via SignalR across all boards in the project
        var boards = await _unitOfWork.Boards.Query().Where(b => b.ProjectId == projectId).ToListAsync();
        foreach (var board in boards)
        {
            await _signalRService.SendMemberJoinedAsync(board.Id, userToAdd.Id, userToAdd.FullName);
        }

        var mapped = _mapper.Map<ProjectMemberResponseDto>(newMember);
        mapped.User = _mapper.Map<DTOs.Auth.UserResponseDto>(userToAdd);
        return mapped;
    }

    public async Task<ProjectMemberResponseDto> UpdateProjectMemberAsync(Guid projectId, Guid userId, Guid memberUserId, UpdateProjectMemberDto dto)
    {
        await ValidateProjectAdminAsync(projectId, userId);

        var membership = await _unitOfWork.ProjectMembers.Query()
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == memberUserId);

        if (membership == null)
        {
            throw new KeyNotFoundException("Member not found in this project.");
        }

        var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
        if (project != null && project.OwnerId == memberUserId && dto.Role != ProjectMemberRole.Owner)
        {
            throw new ArgumentException("The project owner's role cannot be downgraded.");
        }

        membership.Role = dto.Role;
        _unitOfWork.ProjectMembers.Update(membership);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            projectId,
            userId,
            "Member",
            membership.Id,
            "MemberRoleUpdated",
            $"Updated member '{membership.User.FullName}' role to '{dto.Role}'"
        );

        return _mapper.Map<ProjectMemberResponseDto>(membership);
    }

    public async Task RemoveProjectMemberAsync(Guid projectId, Guid userId, Guid memberUserId)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new KeyNotFoundException("Project not found.");
        }

        if (project.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("Only the project owner can remove members from the project.");
        }

        if (project.OwnerId == memberUserId)
        {
            throw new ArgumentException("The project owner cannot be removed from the project.");
        }

        var membership = await _unitOfWork.ProjectMembers.Query()
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == memberUserId);

        if (membership == null)
        {
            throw new KeyNotFoundException("Member not found in this project.");
        }

        _unitOfWork.ProjectMembers.Delete(membership);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            projectId,
            userId,
            "Member",
            membership.Id,
            "MemberRemoved",
            $"Removed '{membership.User.FullName}' from the project"
        );
    }

    public async Task<IEnumerable<ProjectInvitationDto>> GetPendingInvitationsAsync(Guid userId)
    {
        var pendingMembers = await _unitOfWork.ProjectMembers.Query()
            .Include(m => m.Project)
            .ThenInclude(p => p.Owner)
            .Where(m => m.UserId == userId && m.Status == ProjectMemberStatus.Pending)
            .OrderByDescending(m => m.JoinedAt)
            .ToListAsync();

        return pendingMembers.Select(m => new ProjectInvitationDto
        {
            MemberId = m.Id,
            ProjectId = m.ProjectId,
            ProjectName = m.Project.Name,
            ProjectDescription = m.Project.Description,
            InviterName = m.Project.Owner.FullName,
            Role = m.Role,
            InvitedAt = m.JoinedAt
        });
    }

    public async Task AcceptInvitationAsync(Guid memberId, Guid userId)
    {
        var member = await _unitOfWork.ProjectMembers.Query()
            .FirstOrDefaultAsync(m => m.Id == memberId && m.UserId == userId);

        if (member == null)
        {
            throw new KeyNotFoundException("Invitation not found.");
        }

        member.Status = ProjectMemberStatus.Accepted;
        _unitOfWork.ProjectMembers.Update(member);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            member.ProjectId,
            userId,
            "Member",
            member.Id,
            "MemberAcceptedInvitation",
            "Accepted the invitation to join the project."
        );
    }

    public async Task RejectInvitationAsync(Guid memberId, Guid userId)
    {
        var member = await _unitOfWork.ProjectMembers.Query()
            .FirstOrDefaultAsync(m => m.Id == memberId && m.UserId == userId);

        if (member == null)
        {
            throw new KeyNotFoundException("Invitation not found.");
        }

        _unitOfWork.ProjectMembers.Delete(member);
        await _unitOfWork.SaveChangesAsync();
    }
}
