using MediatR;
using Microsoft.Extensions.Logging;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Enums;
using TaskStatus = VolcanionPM.Domain.Enums.TaskStatus;

namespace VolcanionPM.Application.Features.Reporting.Queries.GetProjectProgressReport;

public class GetProjectProgressReportQueryHandler : IRequestHandler<GetProjectProgressReportQuery, Result<ProjectProgressReportDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IRiskRepository _riskRepository;
    private readonly IIssueRepository _issueRepository;
    private readonly IResourceAllocationRepository _resourceAllocationRepository;
    private readonly ILogger<GetProjectProgressReportQueryHandler> _logger;

    public GetProjectProgressReportQueryHandler(
        IProjectRepository projectRepository,
        ITaskRepository taskRepository,
        IRiskRepository riskRepository,
        IIssueRepository issueRepository,
        IResourceAllocationRepository resourceAllocationRepository,
        ILogger<GetProjectProgressReportQueryHandler> logger)
    {
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _riskRepository = riskRepository;
        _issueRepository = issueRepository;
        _resourceAllocationRepository = resourceAllocationRepository;
        _logger = logger;
    }

    public async Task<Result<ProjectProgressReportDto>> Handle(GetProjectProgressReportQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating project progress report for project: {ProjectId}", request.ProjectId);

        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
        {
            return Result<ProjectProgressReportDto>.Failure("Project not found");
        }

        var now = DateTime.UtcNow;
        var tasks = await _taskRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);
        var taskList = tasks.ToList();
        
        var risks = await _riskRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);
        var riskList = risks.ToList();
        
        var issues = await _issueRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);
        var issueList = issues.ToList();
        
        var allocations = await _resourceAllocationRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);
        var allocationList = allocations.ToList();

        // Calculate dates
        var daysRemaining = (project.DateRange.EndDate - now).Days;
        var daysOverdue = now > project.DateRange.EndDate ? (now - project.DateRange.EndDate).Days : 0;

        // Task metrics
        var completedTasks = taskList.Count(t => t.Status == TaskStatus.Done);
        var inProgressTasks = taskList.Count(t => t.Status == TaskStatus.InProgress);
        var blockedTasks = taskList.Count(t => t.Status == TaskStatus.Blocked);
        var taskCompletionRate = taskList.Count > 0 ? (decimal)completedTasks / taskList.Count * 100 : 0;

        // Milestones (using high-priority tasks as milestones)
        var milestones = taskList
            .Where(t => t.Priority == TaskPriority.High)
            .Select(t => new MilestoneDto
            {
                Name = t.Title,
                DueDate = t.DueDate ?? project.DateRange.EndDate,
                IsCompleted = t.Status == TaskStatus.Done,
                Status = t.Status.ToString()
            })
            .OrderBy(m => m.DueDate)
            .ToList();

        var completedMilestones = milestones.Count(m => m.IsCompleted);
        var upcomingMilestones = milestones.Count(m => !m.IsCompleted && m.DueDate >= now);

        // Risks
        var activeRisks = riskList.Count(r => r.Status == RiskStatus.Identified || r.Status == RiskStatus.Analyzing || r.Status == RiskStatus.Mitigating);
        var highPriorityRisks = riskList.Count(r => r.Level == RiskLevel.High || r.Level == RiskLevel.Critical);
        
        var topRisks = riskList
            .Where(r => r.Status == RiskStatus.Identified || r.Status == RiskStatus.Analyzing || r.Status == RiskStatus.Mitigating)
            .OrderByDescending(r => r.Level)
            .Take(5)
            .Select(r => new RiskSummaryDto
            {
                RiskId = r.Id,
                Title = r.Title,
                Severity = r.Level.ToString(),
                Status = r.Status.ToString(),
                ImpactScore = r.Impact
            })
            .ToList();

        // Issues
        var openIssues = issueList.Count(i => i.Status != IssueStatus.Resolved);
        var criticalIssues = issueList.Count(i => i.Severity == IssueSeverity.Critical && i.Status != IssueStatus.Resolved);
        
        var criticalIssuesList = issueList
            .Where(i => i.Severity == IssueSeverity.Critical && i.Status != IssueStatus.Resolved)
            .OrderByDescending(i => i.CreatedAt)
            .Take(5)
            .Select(i => new IssueSummaryDto
            {
                IssueId = i.Id,
                Title = i.Title,
                Severity = i.Severity.ToString(),
                Status = i.Status.ToString(),
                ReportedAt = i.CreatedAt
            })
            .ToList();

        // Team size
        var teamSize = allocationList.Select(a => a.UserId).Distinct().Count();

        // Project health calculation
        var projectHealth = CalculateProjectHealth(
            project.ProgressPercentage,
            daysOverdue,
            blockedTasks,
            criticalIssues,
            highPriorityRisks,
            taskCompletionRate
        );

        // Blockers
        var blockers = new List<string>();
        if (blockedTasks > 0)
            blockers.Add($"{blockedTasks} blocked tasks requiring attention");
        if (criticalIssues > 0)
            blockers.Add($"{criticalIssues} critical issues need resolution");
        if (daysOverdue > 0)
            blockers.Add($"Project is {daysOverdue} days overdue");

        // Recommendations
        var recommendations = new List<string>();
        if (taskCompletionRate < 50 && daysRemaining < 30)
            recommendations.Add("Consider reducing scope or extending timeline");
        if (highPriorityRisks > 3)
            recommendations.Add("Increase risk mitigation efforts");
        if (blockedTasks > taskList.Count * 0.1m)
            recommendations.Add("Focus on unblocking tasks to maintain velocity");

        var report = new ProjectProgressReportDto
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            Status = project.Status.ToString(),
            ProgressPercentage = project.ProgressPercentage,
            StartDate = project.DateRange.StartDate,
            EndDate = project.DateRange.EndDate,
            DaysRemaining = Math.Max(0, daysRemaining),
            DaysOverdue = daysOverdue,
            IsOnTrack = daysOverdue == 0 && project.ProgressPercentage >= GetExpectedProgress(project.DateRange.StartDate, project.DateRange.EndDate, now),
            
            TotalMilestones = milestones.Count,
            CompletedMilestones = completedMilestones,
            UpcomingMilestones = upcomingMilestones,
            Milestones = milestones,
            
            TotalTasks = taskList.Count,
            CompletedTasks = completedTasks,
            InProgressTasks = inProgressTasks,
            BlockedTasks = blockedTasks,
            TaskCompletionRate = taskCompletionRate,
            
            TotalRisks = riskList.Count,
            HighPriorityRisks = highPriorityRisks,
            ActiveRisks = activeRisks,
            TopRisks = topRisks,
            
            TotalIssues = issueList.Count,
            OpenIssues = openIssues,
            CriticalIssues = criticalIssues,
            CriticalIssuesList = criticalIssuesList,
            
            TeamSize = teamSize,
            ProjectManagerName = project.ProjectManager != null ? $"{project.ProjectManager.FirstName} {project.ProjectManager.LastName}" : "Unassigned",
            
            ProjectHealth = projectHealth,
            Blockers = blockers,
            Recommendations = recommendations
        };

        _logger.LogInformation("Project progress report generated: {ProjectName} - Health: {Health}", 
            project.Name, projectHealth);

        return Result<ProjectProgressReportDto>.Success(report);
    }

    private string CalculateProjectHealth(
        decimal progressPercentage,
        int daysOverdue,
        int blockedTasks,
        int criticalIssues,
        int highPriorityRisks,
        decimal taskCompletionRate)
    {
        var score = 0;

        // Negative indicators
        if (daysOverdue > 0) score -= 30;
        if (criticalIssues > 0) score -= 20;
        if (blockedTasks > 5) score -= 15;
        if (highPriorityRisks > 3) score -= 10;
        if (taskCompletionRate < 50) score -= 10;

        // Positive indicators
        if (progressPercentage > 80) score += 30;
        else if (progressPercentage > 50) score += 20;
        else if (progressPercentage > 25) score += 10;

        if (daysOverdue == 0) score += 20;
        if (criticalIssues == 0) score += 15;
        if (blockedTasks == 0) score += 10;

        // Determine health status
        if (score >= 50) return "Green";
        if (score >= 0) return "Yellow";
        return "Red";
    }

    private decimal GetExpectedProgress(DateTime startDate, DateTime endDate, DateTime currentDate)
    {
        var totalDays = (endDate - startDate).TotalDays;
        var elapsedDays = (currentDate - startDate).TotalDays;
        
        if (totalDays <= 0) return 100;
        if (elapsedDays <= 0) return 0;
        
        return (decimal)(Math.Min(elapsedDays / totalDays, 1.0) * 100);
    }
}
