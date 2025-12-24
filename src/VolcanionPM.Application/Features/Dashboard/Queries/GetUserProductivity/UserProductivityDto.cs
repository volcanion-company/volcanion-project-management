namespace VolcanionPM.Application.Features.Dashboard.Queries.GetUserProductivity;

public class UserProductivityDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int AssignedTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public decimal TotalHoursLogged { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal AverageTaskCompletionDays { get; set; }
    public Dictionary<string, decimal> HoursByProject { get; set; } = new();
    public List<TopTaskDto> TopTasks { get; set; } = new();
}

public class TopTaskDto
{
    public Guid TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public decimal HoursLogged { get; set; }
    public string Status { get; set; } = string.Empty;
}
