namespace VolcanionPM.Application.Features.Dashboard.Queries.GetTeamVelocity;

public class TeamVelocityDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal AverageVelocity { get; set; }
    public List<SprintVelocityDto> SprintVelocities { get; set; } = new();
}

public class SprintVelocityDto
{
    public Guid SprintId { get; set; }
    public string SprintName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int PlannedStoryPoints { get; set; }
    public int CompletedStoryPoints { get; set; }
    public decimal CompletionRate { get; set; }
}
