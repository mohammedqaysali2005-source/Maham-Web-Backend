using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Maham.Domain.Entities;
using Maham.Domain.Interfaces;
using Maham.Domain.Enums;
using Maham.Application.DTOs.Dashboard;
using Maham.Application.DTOs.Activity;
using Maham.Application.Services.Interfaces;

namespace Maham.Application.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DashboardService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    private async Task ValidateProjectAccessAsync(Guid projectId, Guid userId)
    {
        var isOwner = await _unitOfWork.Projects.Query()
            .AnyAsync(p => p.Id == projectId && p.OwnerId == userId);
        
        var isMember = await _unitOfWork.ProjectMembers.Query()
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);

        if (!isOwner && !isMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this project.");
        }
    }

    public async Task<DashboardStatsDto> GetGeneralDashboardAsync(Guid userId)
    {
        // General dashboard covers all cards assigned to the user
        var cardsQuery = _unitOfWork.Cards.Query()
            .Where(c => c.AssigneeId == userId);

        var now = DateTime.UtcNow;

        var totalCards = await cardsQuery.CountAsync();
        var doneCards = await cardsQuery.CountAsync(c => c.Status == CardStatus.Done);
        var inProgressCards = await cardsQuery.CountAsync(c => c.Status == CardStatus.InProgress);
        var overdueCards = await cardsQuery.CountAsync(c => c.Status != CardStatus.Done && c.DueDate.HasValue && c.DueDate.Value < now);

        var completionRate = totalCards > 0 ? Math.Round((double)doneCards / totalCards * 100, 2) : 0;

        var low = await cardsQuery.CountAsync(c => c.Priority == Priority.Low);
        var medium = await cardsQuery.CountAsync(c => c.Priority == Priority.Medium);
        var high = await cardsQuery.CountAsync(c => c.Priority == Priority.High);
        var critical = await cardsQuery.CountAsync(c => c.Priority == Priority.Critical);

        // Recent activity for projects this user is member of
        var userProjectIds = await _unitOfWork.ProjectMembers.Query()
            .Where(m => m.UserId == userId)
            .Select(m => m.ProjectId)
            .ToListAsync();

        var ownedProjectIds = await _unitOfWork.Projects.Query()
            .Where(p => p.OwnerId == userId)
            .Select(p => p.Id)
            .ToListAsync();

        var allProjectIds = userProjectIds.Union(ownedProjectIds).Distinct().ToList();

        var recentLogs = await _unitOfWork.ActivityLogs.Query()
            .Include(a => a.User)
            .Where(a => allProjectIds.Contains(a.ProjectId))
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .ToListAsync();

        return new DashboardStatsDto
        {
            TotalCards = totalCards,
            DoneCards = doneCards,
            InProgressCards = inProgressCards,
            OverdueCards = overdueCards,
            CompletionRate = completionRate,
            ByPriority = new PriorityBreakdownDto
            {
                Low = low,
                Medium = medium,
                High = high,
                Critical = critical
            },
            ByMember = new List<MemberCardCountDto>(), // Not applicable for general user dashboard
            RecentActivity = _mapper.Map<IEnumerable<ActivityLogResponseDto>>(recentLogs)
        };
    }

    public async Task<DashboardStatsDto> GetProjectDashboardAsync(Guid projectId, Guid userId)
    {
        await ValidateProjectAccessAsync(projectId, userId);

        var cardsQuery = _unitOfWork.Cards.Query()
            .Where(c => c.Column.Board.ProjectId == projectId);

        var now = DateTime.UtcNow;

        var totalCards = await cardsQuery.CountAsync();
        var doneCards = await cardsQuery.CountAsync(c => c.Status == CardStatus.Done);
        var inProgressCards = await cardsQuery.CountAsync(c => c.Status == CardStatus.InProgress);
        var overdueCards = await cardsQuery.CountAsync(c => c.Status != CardStatus.Done && c.DueDate.HasValue && c.DueDate.Value < now);

        var completionRate = totalCards > 0 ? Math.Round((double)doneCards / totalCards * 100, 2) : 0;

        var low = await cardsQuery.CountAsync(c => c.Priority == Priority.Low);
        var medium = await cardsQuery.CountAsync(c => c.Priority == Priority.Medium);
        var high = await cardsQuery.CountAsync(c => c.Priority == Priority.High);
        var critical = await cardsQuery.CountAsync(c => c.Priority == Priority.Critical);

        // Team card count breakdown
        var members = await _unitOfWork.ProjectMembers.Query()
            .Include(m => m.User)
            .Where(m => m.ProjectId == projectId)
            .ToListAsync();

        var byMember = new List<MemberCardCountDto>();
        foreach (var member in members)
        {
            var cardCount = await cardsQuery.CountAsync(c => c.AssigneeId == member.UserId);
            byMember.Add(new MemberCardCountDto
            {
                UserId = member.UserId,
                Name = member.User.FullName,
                CardCount = cardCount
            });
        }

        var recentLogs = await _unitOfWork.ActivityLogs.Query()
            .Include(a => a.User)
            .Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .ToListAsync();

        return new DashboardStatsDto
        {
            TotalCards = totalCards,
            DoneCards = doneCards,
            InProgressCards = inProgressCards,
            OverdueCards = overdueCards,
            CompletionRate = completionRate,
            ByPriority = new PriorityBreakdownDto
            {
                Low = low,
                Medium = medium,
                High = high,
                Critical = critical
            },
            ByMember = byMember,
            RecentActivity = _mapper.Map<IEnumerable<ActivityLogResponseDto>>(recentLogs)
        };
    }

    public async Task<PublicStatsDto> GetPublicStatsAsync()
    {
        var totalProjects = await _unitOfWork.Projects.Query().CountAsync();
        var totalUsers = await _unitOfWork.Users.Query().CountAsync();
        var totalCompletedCards = await _unitOfWork.Cards.Query().CountAsync(c => c.Status == CardStatus.Done);
        var totalActiveBoards = await _unitOfWork.Boards.Query().CountAsync();

        return new PublicStatsDto
        {
            TotalProjects = totalProjects,
            TotalUsers = totalUsers,
            TotalCompletedCards = totalCompletedCards,
            TotalActiveBoards = totalActiveBoards
        };
    }

    public async Task<ChartDataDto> GetDashboardChartsAsync(Guid userId)
    {
        var cardsQuery = _unitOfWork.Cards.Query().Where(c => c.AssigneeId == userId);

        var todoCount = await cardsQuery.CountAsync(c => c.Status == CardStatus.Todo);
        var inProgressCount = await cardsQuery.CountAsync(c => c.Status == CardStatus.InProgress);
        var doneCount = await cardsQuery.CountAsync(c => c.Status == CardStatus.Done);

        var lowCount = await cardsQuery.CountAsync(c => c.Priority == Priority.Low);
        var mediumCount = await cardsQuery.CountAsync(c => c.Priority == Priority.Medium);
        var highCount = await cardsQuery.CountAsync(c => c.Priority == Priority.High);
        var criticalCount = await cardsQuery.CountAsync(c => c.Priority == Priority.Critical);

        // Activity last 7 days
        var startDate = DateTime.UtcNow.Date.AddDays(-6);
        var activities = await _unitOfWork.ActivityLogs.Query()
            .Where(a => a.UserId == userId && a.CreatedAt >= startDate)
            .ToListAsync();

        var dailyActivities = new List<DailyActivityDto>();
        for (int i = 0; i < 7; i++)
        {
            var day = startDate.AddDays(i);
            var count = activities.Count(a => a.CreatedAt.Date == day);
            dailyActivities.Add(new DailyActivityDto
            {
                Date = day.ToString("yyyy-MM-dd"),
                Count = count
            });
        }

        return new ChartDataDto
        {
            StatusDistribution = new Dictionary<string, int>
            {
                { "قيد الانتظار (Todo)", todoCount },
                { "جاري العمل (InProgress)", inProgressCount },
                { "مكتملة (Done)", doneCount }
            },
            PriorityDistribution = new Dictionary<string, int>
            {
                { "منخفضة (Low)", lowCount },
                { "متوسطة (Medium)", mediumCount },
                { "عالية (High)", highCount },
                { "حرجة (Critical)", criticalCount }
            },
            ActivityLast7Days = dailyActivities
        };
    }
}
