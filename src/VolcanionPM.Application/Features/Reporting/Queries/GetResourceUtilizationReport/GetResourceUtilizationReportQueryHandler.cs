using MediatR;
using Microsoft.Extensions.Logging;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using TaskStatus = VolcanionPM.Domain.Enums.TaskStatus;

namespace VolcanionPM.Application.Features.Reporting.Queries.GetResourceUtilizationReport;

public class GetResourceUtilizationReportQueryHandler : IRequestHandler<GetResourceUtilizationReportQuery, Result<ResourceUtilizationReportDto>>
{
    private readonly IResourceAllocationRepository _allocationRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<GetResourceUtilizationReportQueryHandler> _logger;

    private const decimal STANDARD_HOURS_PER_WEEK = 40m;

    public GetResourceUtilizationReportQueryHandler(
        IResourceAllocationRepository allocationRepository,
        ITaskRepository taskRepository,
        ITimeEntryRepository timeEntryRepository,
        IProjectRepository projectRepository,
        ILogger<GetResourceUtilizationReportQueryHandler> logger)
    {
        _allocationRepository = allocationRepository;
        _taskRepository = taskRepository;
        _timeEntryRepository = timeEntryRepository;
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<Result<ResourceUtilizationReportDto>> Handle(GetResourceUtilizationReportQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating resource utilization report for project: {ProjectId}, organization: {OrgId}", 
            request.ProjectId, request.OrganizationId);

        var startDate = request.StartDate ?? DateTime.UtcNow;
        var endDate = request.EndDate ?? DateTime.UtcNow.AddMonths(1);
        
        var now = DateTime.UtcNow;
        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
        var weekEnd = weekStart.AddDays(7);
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        // Get allocations
        IEnumerable<Domain.Entities.ResourceAllocation> allocations;
        if (request.ProjectId.HasValue)
        {
            allocations = await _allocationRepository.GetByProjectIdAsync(request.ProjectId.Value, cancellationToken);
        }
        else
        {
            allocations = await _allocationRepository.GetAllAsync(cancellationToken);
        }

        var allocationList = allocations
            .Where(a => a.AllocationPeriod.StartDate <= endDate && a.AllocationPeriod.EndDate >= startDate)
            .ToList();

        // Group by user
        var userAllocations = allocationList.GroupBy(a => a.UserId);

        var resources = new List<ResourceDetailDto>();
        var totalAllocatedHours = 0m;
        var totalAvailableHours = 0m;

        foreach (var userGroup in userAllocations)
        {
            var userId = userGroup.Key;
            var userAllocs = userGroup.ToList();
            var user = userAllocs.First().User;

            // Get tasks assigned to user
            var tasks = await _taskRepository.GetByAssignedUserIdAsync(userId, cancellationToken);
            var taskList = tasks.ToList();
            
            var assignedTasks = taskList.Count;
            var completedTasks = taskList.Count(t => t.Status == TaskStatus.Done);
            var inProgressTasks = taskList.Count(t => t.Status == TaskStatus.InProgress);

            // Get time entries
            var timeEntries = await _timeEntryRepository.GetByUserIdAsync(userId, cancellationToken);
            var timeEntryList = timeEntries.ToList();
            
            var hoursThisWeek = timeEntryList
                .Where(te => te.Date >= weekStart && te.Date < weekEnd)
                .Sum(te => te.Hours);
            
            var hoursThisMonth = timeEntryList
                .Where(te => te.Date >= monthStart && te.Date < monthEnd)
                .Sum(te => te.Hours);

            // Calculate allocation (convert allocation percentage to hours per week)
            var allocatedHoursPerWeek = userAllocs.Sum(a => (a.AllocationPercentage / 100m) * STANDARD_HOURS_PER_WEEK);
            var weeksInPeriod = Math.Max(1, (endDate - startDate).Days / 7);
            var userTotalAllocatedHours = allocatedHoursPerWeek * weeksInPeriod;
            var userAvailableHours = STANDARD_HOURS_PER_WEEK * weeksInPeriod;
            var utilizationPercentage = userAvailableHours > 0 
                ? (allocatedHoursPerWeek / STANDARD_HOURS_PER_WEEK) * 100 
                : 0;

            // Determine allocation status
            var allocationStatus = utilizationPercentage switch
            {
                > 100 => "Overallocated",
                >= 80 => "Optimal",
                >= 50 => "Underutilized",
                _ => "Available"
            };

            // Project allocations
            var projectAllocations = userAllocs.Select(a => new ProjectAllocationDto
            {
                ProjectId = a.ProjectId,
                ProjectName = a.Project.Name,
                AllocatedHoursPerWeek = (a.AllocationPercentage / 100m) * STANDARD_HOURS_PER_WEEK,
                Role = a.Type.ToString(),
                StartDate = a.AllocationPeriod.StartDate,
                EndDate = a.AllocationPeriod.EndDate
            }).ToList();

            resources.Add(new ResourceDetailDto
            {
                UserId = userId,
                UserName = $"{user.FirstName} {user.LastName}",
                Role = user.Role.ToString(),
                AllocatedHoursPerWeek = allocatedHoursPerWeek,
                TotalAllocatedHours = userTotalAllocatedHours,
                AvailableHoursPerWeek = STANDARD_HOURS_PER_WEEK,
                UtilizationPercentage = utilizationPercentage,
                AssignedTasks = assignedTasks,
                CompletedTasks = completedTasks,
                InProgressTasks = inProgressTasks,
                HoursLoggedThisWeek = hoursThisWeek,
                HoursLoggedThisMonth = hoursThisMonth,
                AllocationStatus = allocationStatus,
                ProjectAllocations = projectAllocations
            });

            totalAllocatedHours += userTotalAllocatedHours;
            totalAvailableHours += userAvailableHours;
        }

        // Calculate overall metrics
        var averageUtilization = resources.Any() 
            ? resources.Average(r => r.UtilizationPercentage) 
            : 0;
        
        var utilizationRate = totalAvailableHours > 0 
            ? (totalAllocatedHours / totalAvailableHours) * 100 
            : 0;

        var overallocatedCount = resources.Count(r => r.UtilizationPercentage > 100);
        var underutilizedCount = resources.Count(r => r.UtilizationPercentage < 80);
        var optimalCount = resources.Count(r => r.UtilizationPercentage >= 80 && r.UtilizationPercentage <= 100);

        // Generate warnings and suggestions
        var warnings = new List<string>();
        var suggestions = new List<string>();

        if (overallocatedCount > 0)
            warnings.Add($"{overallocatedCount} resources are overallocated (>100% capacity)");
        
        if (underutilizedCount > resources.Count * 0.3m)
            warnings.Add($"{underutilizedCount} resources are underutilized (<80% capacity)");

        if (overallocatedCount > 0)
            suggestions.Add("Consider redistributing tasks from overallocated resources");
        
        if (underutilizedCount > 0 && overallocatedCount > 0)
            suggestions.Add("Balance workload between overallocated and underutilized resources");

        // Get project name if filtering by project
        string? projectName = null;
        if (request.ProjectId.HasValue)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId.Value, cancellationToken);
            projectName = project?.Name;
        }

        var report = new ResourceUtilizationReportDto
        {
            ProjectId = request.ProjectId,
            ProjectName = projectName,
            StartDate = startDate,
            EndDate = endDate,
            TotalResources = resources.Count,
            AverageUtilization = averageUtilization,
            TotalAllocatedHours = totalAllocatedHours,
            TotalAvailableHours = totalAvailableHours,
            UtilizationRate = utilizationRate,
            Resources = resources.OrderByDescending(r => r.UtilizationPercentage).ToList(),
            OverallocatedResources = overallocatedCount,
            UnderutilizedResources = underutilizedCount,
            OptimallyAllocatedResources = optimalCount,
            CapacityWarnings = warnings,
            ReallocationSuggestions = suggestions
        };

        _logger.LogInformation("Resource utilization report generated: {TotalResources} resources, {AvgUtilization}% avg utilization", 
            report.TotalResources, report.AverageUtilization);

        return Result<ResourceUtilizationReportDto>.Success(report);
    }
}
