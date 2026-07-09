using System;
using System.Collections.Generic;
using Maham.Application.DTOs.Activity;

namespace Maham.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TotalCards { get; set; }
    public int DoneCards { get; set; }
    public int InProgressCards { get; set; }
    public int OverdueCards { get; set; }
    public double CompletionRate { get; set; }
    public PriorityBreakdownDto ByPriority { get; set; } = null!;
    public IEnumerable<MemberCardCountDto> ByMember { get; set; } = new List<MemberCardCountDto>();
    public IEnumerable<ActivityLogResponseDto> RecentActivity { get; set; } = new List<ActivityLogResponseDto>();
}

public class PriorityBreakdownDto
{
    public int Low { get; set; }
    public int Medium { get; set; }
    public int High { get; set; }
    public int Critical { get; set; }
}

public class MemberCardCountDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public int CardCount { get; set; }
}

public class PublicStatsDto
{
    public int TotalProjects { get; set; }
    public int TotalUsers { get; set; }
    public int TotalCompletedCards { get; set; }
    public int TotalActiveBoards { get; set; }
}

public class ChartDataDto
{
    public Dictionary<string, int> StatusDistribution { get; set; } = new();
    public Dictionary<string, int> PriorityDistribution { get; set; } = new();
    public List<DailyActivityDto> ActivityLast7Days { get; set; } = new();
}

public class DailyActivityDto
{
    public string Date { get; set; } = null!;
    public int Count { get; set; }
}
