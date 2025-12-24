namespace VolcanionPM.Application.Features.Dashboard.Queries.GetDashboardOverview;

public class DashboardOverviewDto
{
    public ProjectStatisticsDto ProjectStatistics { get; set; } = new();
    public TaskStatisticsDto TaskStatistics { get; set; } = new();
    public TimeTrackingDto TimeTracking { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class TimeTrackingDto
{
    public decimal TotalHoursThisWeek { get; set; }
    public decimal TotalHoursThisMonth { get; set; }
    public decimal BillableHoursThisMonth { get; set; }
    public decimal AverageHoursPerDay { get; set; }
}

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ProjectStatisticsDto
{
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int OverdueProjects { get; set; }
}

public class TaskStatisticsDto
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int OverdueTasks { get; set; }
    public decimal CompletionRate { get; set; }
}
