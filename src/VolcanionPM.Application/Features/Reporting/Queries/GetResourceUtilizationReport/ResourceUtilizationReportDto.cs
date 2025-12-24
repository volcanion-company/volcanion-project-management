namespace VolcanionPM.Application.Features.Reporting.Queries.GetResourceUtilizationReport;

public class ResourceUtilizationReportDto
{
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    // Overall metrics
    public int TotalResources { get; set; }
    public decimal AverageUtilization { get; set; }
    public decimal TotalAllocatedHours { get; set; }
    public decimal TotalAvailableHours { get; set; }
    public decimal UtilizationRate { get; set; }
    
    // Resource breakdown
    public List<ResourceDetailDto> Resources { get; set; } = new();
    
    // Capacity analysis
    public int OverallocatedResources { get; set; }
    public int UnderutilizedResources { get; set; }
    public int OptimallyAllocatedResources { get; set; }
    
    // Recommendations
    public List<string> CapacityWarnings { get; set; } = new();
    public List<string> ReallocationSuggestions { get; set; } = new();
}

public class ResourceDetailDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    
    // Allocation
    public decimal AllocatedHoursPerWeek { get; set; }
    public decimal TotalAllocatedHours { get; set; }
    public decimal AvailableHoursPerWeek { get; set; }
    public decimal UtilizationPercentage { get; set; }
    
    // Tasks
    public int AssignedTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    
    // Time tracking
    public decimal HoursLoggedThisWeek { get; set; }
    public decimal HoursLoggedThisMonth { get; set; }
    
    // Status
    public string AllocationStatus { get; set; } = string.Empty; // Overallocated, Optimal, Underutilized
    public List<ProjectAllocationDto> ProjectAllocations { get; set; } = new();
}

public class ProjectAllocationDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal AllocatedHoursPerWeek { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
