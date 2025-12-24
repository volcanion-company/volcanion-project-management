namespace VolcanionPM.Application.Features.Dashboard.Queries.GetTimeDistribution;

public class TimeDistributionDto
{
    public Guid? UserId { get; set; }
    public Guid? ProjectId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalHours { get; set; }
    public Dictionary<string, decimal> HoursByProject { get; set; } = new();
    public Dictionary<string, decimal> HoursByTask { get; set; } = new();
    public Dictionary<string, decimal> HoursByDay { get; set; } = new();
    public decimal BillableHours { get; set; }
    public decimal NonBillableHours { get; set; }
}
