namespace VolcanionPM.Application.Features.Dashboard.Queries.GetSprintBurndown;

public class SprintBurndownDto
{
    public Guid SprintId { get; set; }
    public string SprintName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalStoryPoints { get; set; }
    public List<BurndownDataPoint> IdealBurndown { get; set; } = new();
    public List<BurndownDataPoint> ActualBurndown { get; set; } = new();
}

public class BurndownDataPoint
{
    public DateTime Date { get; set; }
    public int RemainingStoryPoints { get; set; }
}
