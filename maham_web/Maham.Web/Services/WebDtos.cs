using System;
using System.Collections.Generic;

namespace Maham.Web.Services;

public class WebUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "Member";
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WebAuthResponseDto
{
    public string Token { get; set; } = "";
    public WebUserDto User { get; set; } = new();
}

public class WebApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
}

public class WebProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
    public int BoardCount { get; set; }
}

public class WebBoardDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? BackgroundColor { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<WebColumnDto> Columns { get; set; } = new();
}

public class WebColumnDto
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public string Name { get; set; } = "";
    public int Order { get; set; }
    public int? WipLimit { get; set; }
    public List<WebCardDto> Cards { get; set; } = new();
}

public class WebCardDto
{
    public Guid Id { get; set; }
    public Guid ColumnId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Todo";
    public Guid? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
    public DateTime? DueDate { get; set; }
    public string? CoverImageUrl { get; set; }
    public int Order { get; set; }
    public int? StoryPoints { get; set; }
    public string? Labels { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WebCommentDto
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = "";
    public string Content { get; set; } = "";
    public bool IsEdited { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WebAttachmentDto
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public string FileName { get; set; } = "";
    public long FileSize { get; set; }
    public string MimeType { get; set; } = "";
    public string UploadedByName { get; set; } = "";
    public DateTime UploadedAt { get; set; }
}

public class WebMemberDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public WebUserDto? User { get; set; }

    private string _userName = "";
    public string UserName
    {
        get => !string.IsNullOrEmpty(_userName) ? _userName : (User?.FullName ?? "");
        set => _userName = value;
    }

    private string _userEmail = "";
    public string UserEmail
    {
        get => !string.IsNullOrEmpty(_userEmail) ? _userEmail : (User?.Email ?? "");
        set => _userEmail = value;
    }

    public string Role { get; set; } = "Member";
    public DateTime JoinedAt { get; set; }
}

public class WebActivityDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = "";
    public string Description { get; set; } = "";
    public string UserName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class WebDashboardDto
{
    public int TotalCards { get; set; }
    public int DoneCards { get; set; }
    public int OverdueCards { get; set; }
    public int InProgressCards { get; set; }
    public double CompletionRate { get; set; }
    public int TotalProjects { get; set; }
    public List<WebActivityDto> RecentActivity { get; set; } = new();
}

public class WebPagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class WebPublicStatsDto
{
    public int TotalProjects { get; set; }
    public int TotalUsers { get; set; }
    public int TotalCompletedCards { get; set; }
    public int TotalActiveBoards { get; set; }
}

public class WebChartDataDto
{
    public Dictionary<string, int> StatusDistribution { get; set; } = new();
    public Dictionary<string, int> PriorityDistribution { get; set; } = new();
    public List<WebDailyActivityDto> ActivityLast7Days { get; set; } = new();
}

public class WebDailyActivityDto
{
    public string Date { get; set; } = "";
    public int Count { get; set; }
}

public class WebProjectInvitationDto
{
    public Guid MemberId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = "";
    public string? ProjectDescription { get; set; }
    public string InviterName { get; set; } = "";
    public string Role { get; set; } = "Member";
    public DateTime InvitedAt { get; set; }
}
