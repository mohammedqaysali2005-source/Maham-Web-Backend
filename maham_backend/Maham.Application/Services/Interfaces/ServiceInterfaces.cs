using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Maham.Application.DTOs.Common;
using Maham.Application.DTOs.Auth;
using Maham.Application.DTOs.Project;
using Maham.Application.DTOs.Board;
using Maham.Application.DTOs.Column;
using Maham.Application.DTOs.Card;
using Maham.Application.DTOs.Comment;
using Maham.Application.DTOs.Attachment;
using Maham.Application.DTOs.Dashboard;
using Maham.Application.DTOs.Activity;

namespace Maham.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<UserResponseDto> GetMeAsync(Guid userId);
    Task<UserResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(Guid userId);
}

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(Guid currentUserId);
    Task<UserResponseDto> GetUserByIdAsync(Guid userId);
    Task<IEnumerable<UserResponseDto>> SearchUsersAsync(string query);
    Task<UserResponseDto> UploadAvatarAsync(Guid userId, IFormFile file);
    Task<UserResponseDto> UpdateUserRoleAsync(Guid userId, string newRole, Guid currentUserId);
    Task DeleteUserAsync(Guid userId, Guid currentUserId);
}

public interface IProjectService
{
    Task<IEnumerable<ProjectResponseDto>> GetUserProjectsAsync(Guid userId);
    Task<ProjectResponseDto> CreateProjectAsync(Guid userId, CreateProjectDto dto);
    Task<ProjectResponseDto> GetProjectByIdAsync(Guid projectId, Guid userId);
    Task<ProjectResponseDto> UpdateProjectAsync(Guid projectId, Guid userId, UpdateProjectDto dto);
    Task DeleteProjectAsync(Guid projectId, Guid userId);
    Task<IEnumerable<ProjectMemberResponseDto>> GetProjectMembersAsync(Guid projectId, Guid userId);
    Task<ProjectMemberResponseDto> AddProjectMemberAsync(Guid projectId, Guid userId, AddProjectMemberDto dto);
    Task<ProjectMemberResponseDto> UpdateProjectMemberAsync(Guid projectId, Guid userId, Guid memberUserId, UpdateProjectMemberDto dto);
    Task RemoveProjectMemberAsync(Guid projectId, Guid userId, Guid memberUserId);
    Task<IEnumerable<ProjectInvitationDto>> GetPendingInvitationsAsync(Guid userId);
    Task AcceptInvitationAsync(Guid memberId, Guid userId);
    Task RejectInvitationAsync(Guid memberId, Guid userId);
}

public interface IBoardService
{
    Task<IEnumerable<BoardResponseDto>> GetProjectBoardsAsync(Guid projectId, Guid userId);
    Task<BoardResponseDto> CreateBoardAsync(Guid projectId, Guid userId, CreateBoardDto dto);
    Task<BoardDetailsDto> GetBoardDetailsAsync(Guid boardId, Guid userId);
    Task<BoardResponseDto> UpdateBoardAsync(Guid boardId, Guid userId, UpdateBoardDto dto);
    Task DeleteBoardAsync(Guid boardId, Guid userId);
    Task<string> ExportBoardToJsonAsync(Guid boardId, Guid userId);
}

public interface IColumnService
{
    Task<ColumnResponseDto> GetColumnByIdAsync(Guid columnId, Guid userId);
    Task<ColumnResponseDto> CreateColumnAsync(Guid boardId, Guid userId, CreateColumnDto dto);
    Task<ColumnResponseDto> UpdateColumnAsync(Guid columnId, Guid userId, UpdateColumnDto dto);
    Task DeleteColumnAsync(Guid columnId, Guid userId);
    Task ReorderColumnsAsync(Guid boardId, Guid userId, IEnumerable<ColumnReorderDto> dtos);
}

public interface ICardService
{
    Task<CardResponseDto> GetCardByIdAsync(Guid cardId, Guid userId);
    Task<CardResponseDto> CreateCardAsync(Guid columnId, Guid userId, CreateCardDto dto);
    Task<CardResponseDto> UpdateCardAsync(Guid cardId, Guid userId, UpdateCardDto dto);
    Task DeleteCardAsync(Guid cardId, Guid userId);
    Task<CardResponseDto> MoveCardAsync(Guid cardId, Guid userId, MoveCardDto dto);
    Task<CardResponseDto> AssignCardAsync(Guid cardId, Guid userId, AssignCardDto dto);
    Task<CardResponseDto> UpdatePriorityAsync(Guid cardId, Guid userId, UpdatePriorityDto dto);
    Task<CardResponseDto> UpdateDueDateAsync(Guid cardId, Guid userId, UpdateDueDateDto dto);
    Task<CardResponseDto> UpdateStatusAsync(Guid cardId, Guid userId, UpdateStatusDto dto);
    Task<CardResponseDto> UpdateLabelsAsync(Guid cardId, Guid userId, UpdateLabelsDto dto);
    Task<CardResponseDto> UploadCoverAsync(Guid cardId, Guid userId, IFormFile file);
    Task DeleteCoverAsync(Guid cardId, Guid userId);
    Task<IEnumerable<CardResponseDto>> GetUserAssignedCardsAsync(Guid userId);
    Task<IEnumerable<CardResponseDto>> GetUserCardsDueTodayAsync(Guid userId);
    Task<IEnumerable<CardResponseDto>> GetProjectCardsAsync(Guid projectId, Guid userId);
    Task<IEnumerable<CardResponseDto>> GetProjectOverdueCardsAsync(Guid projectId, Guid userId);
    Task ReorderCardsAsync(Guid columnId, Guid userId, IEnumerable<CardReorderDto> dtos);
}

public interface ICommentService
{
    Task<CommentResponseDto> GetCommentByIdAsync(Guid commentId, Guid userId);
    Task<IEnumerable<CommentResponseDto>> GetCardCommentsAsync(Guid cardId, Guid userId);
    Task<CommentResponseDto> CreateCommentAsync(Guid cardId, Guid userId, CreateCommentDto dto);
    Task<CommentResponseDto> UpdateCommentAsync(Guid commentId, Guid userId, UpdateCommentDto dto);
    Task DeleteCommentAsync(Guid commentId, Guid userId);
}

public interface IAttachmentService
{
    Task<AttachmentResponseDto> GetAttachmentByIdAsync(Guid attachmentId, Guid userId);
    Task<IEnumerable<AttachmentResponseDto>> GetCardAttachmentsAsync(Guid cardId, Guid userId);
    Task<AttachmentResponseDto> UploadAttachmentAsync(Guid cardId, Guid userId, IFormFile file);
    Task<(byte[] content, string fileName, string contentType)> DownloadAttachmentAsync(Guid attachmentId, Guid userId);
    Task DeleteAttachmentAsync(Guid attachmentId, Guid userId);
}

public interface IActivityService
{
    Task<PagedResponseDto<ActivityLogResponseDto>> GetProjectActivityAsync(Guid projectId, Guid userId, int pageNumber, int pageSize);
    Task<IEnumerable<ActivityLogResponseDto>> GetBoardActivityAsync(Guid boardId, Guid userId);
    Task<IEnumerable<ActivityLogResponseDto>> GetCardActivityAsync(Guid cardId, Guid userId);
    Task<IEnumerable<ActivityLogResponseDto>> GetUserActivityAsync(Guid userId);
    Task LogActivityAsync(Guid projectId, Guid userId, string entityType, Guid entityId, string action, string description, string? metadata = null);
}

public interface IDashboardService
{
    Task<DashboardStatsDto> GetGeneralDashboardAsync(Guid userId);
    Task<DashboardStatsDto> GetProjectDashboardAsync(Guid projectId, Guid userId);
    Task<PublicStatsDto> GetPublicStatsAsync();
    Task<ChartDataDto> GetDashboardChartsAsync(Guid userId);
}
