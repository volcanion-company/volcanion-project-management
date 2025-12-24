namespace VolcanionPM.Application.Features.Reporting.Queries.GetProjectProgressReport;

public class ProjectProgressReportDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal ProgressPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysRemaining { get; set; }
    public int DaysOverdue { get; set; }
    public bool IsOnTrack { get; set; }
    
    // Milestones
    public int TotalMilestones { get; set; }
    public int CompletedMilestones { get; set; }
    public int UpcomingMilestones { get; set; }
    public List<MilestoneDto> Milestones { get; set; } = new();
    
    // Tasks Summary
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int BlockedTasks { get; set; }
    public decimal TaskCompletionRate { get; set; }
    
    // Risks
    public int TotalRisks { get; set; }
    public int HighPriorityRisks { get; set; }
    public int ActiveRisks { get; set; }
    public List<RiskSummaryDto> TopRisks { get; set; } = new();
    
    // Issues
    public int TotalIssues { get; set; }
    public int OpenIssues { get; set; }
    public int CriticalIssues { get; set; }
    public List<IssueSummaryDto> CriticalIssuesList { get; set; } = new();
    
    // Team
    public int TeamSize { get; set; }
    public string ProjectManagerName { get; set; } = string.Empty;
    
    // Health Indicators
    public string ProjectHealth { get; set; } = string.Empty; // Green, Yellow, Red
    public List<string> Blockers { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class MilestoneDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class RiskSummaryDto
{
    public Guid RiskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal ImpactScore { get; set; }
}

public class IssueSummaryDto
{
    public Guid IssueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ReportedAt { get; set; }
}
