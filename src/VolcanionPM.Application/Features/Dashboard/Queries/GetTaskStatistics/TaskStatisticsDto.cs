namespace VolcanionPM.Application.Features.Dashboard.Queries.GetTaskStatistics;

public class TaskStatisticsDto
{
    public int TotalTasks { get; set; }
    public int ToDoTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int BlockedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal AverageEstimatedHours { get; set; }
    public decimal AverageActualHours { get; set; }
    public int UnassignedTasks { get; set; }
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
    public Dictionary<string, int> TasksByStatus { get; set; } = new();
    public Dictionary<string, int> TasksByType { get; set; } = new();
}
