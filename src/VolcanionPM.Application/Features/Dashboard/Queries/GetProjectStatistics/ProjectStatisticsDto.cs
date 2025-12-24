namespace VolcanionPM.Application.Features.Dashboard.Queries.GetProjectStatistics;

public class ProjectStatisticsDto
{
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int OnHoldProjects { get; set; }
    public int CancelledProjects { get; set; }
    public decimal AverageProgressPercentage { get; set; }
    public int OverdueProjects { get; set; }
    public int ProjectsStartingThisMonth { get; set; }
    public int ProjectsEndingThisMonth { get; set; }
    public Dictionary<string, int> ProjectsByPriority { get; set; } = new();
    public Dictionary<string, int> ProjectsByStatus { get; set; } = new();
}
