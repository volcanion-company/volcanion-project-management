namespace VolcanionPM.Application.Features.Reporting.Queries.GetTimeCostReport;

public class TimeCostReportDto
{
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    // Budget
    public decimal Budget { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal ActualCost { get; set; }
    public decimal CostVariance { get; set; }
    public decimal CostVariancePercentage { get; set; }
    public bool IsOverBudget { get; set; }
    
    // Time tracking
    public decimal TotalHoursLogged { get; set; }
    public decimal TotalEstimatedHours { get; set; }
    public decimal TimeVariance { get; set; }
    public decimal TimeVariancePercentage { get; set; }
    
    // Billable vs Non-billable
    public decimal BillableHours { get; set; }
    public decimal NonBillableHours { get; set; }
    public decimal BillablePercentage { get; set; }
    public decimal BillableRevenue { get; set; }
    
    // Cost breakdown
    public Dictionary<string, decimal> CostByResource { get; set; } = new();
    public Dictionary<string, decimal> HoursByResource { get; set; } = new();
    
    // Profitability
    public decimal TotalRevenue { get; set; }
    public decimal Profit { get; set; }
    public decimal ProfitMargin { get; set; }
    
    // Efficiency metrics
    public decimal CostPerHour { get; set; }
    public decimal RevenuePerHour { get; set; }
    public decimal EfficiencyRatio { get; set; }
    
    // Trend analysis
    public List<CostTrendDto> WeeklyCostTrend { get; set; } = new();
    public List<TimeTrendDto> WeeklyTimeTrend { get; set; } = new();
    
    // Warnings and insights
    public List<string> BudgetWarnings { get; set; } = new();
    public List<string> CostInsights { get; set; } = new();
}

public class CostTrendDto
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public decimal ActualCost { get; set; }
    public decimal PlannedCost { get; set; }
}

public class TimeTrendDto
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public decimal HoursLogged { get; set; }
    public decimal PlannedHours { get; set; }
}
