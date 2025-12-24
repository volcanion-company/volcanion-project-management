using MediatR;
using Microsoft.Extensions.Logging;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Enums;
using TaskStatus = VolcanionPM.Domain.Enums.TaskStatus;

namespace VolcanionPM.Application.Features.Dashboard.Queries.GetDashboardOverview;

public class GetDashboardOverviewQueryHandler : IRequestHandler<GetDashboardOverviewQuery, Result<DashboardOverviewDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly ILogger<GetDashboardOverviewQueryHandler> _logger;

    public GetDashboardOverviewQueryHandler(
        IProjectRepository projectRepository,
        ITaskRepository taskRepository,
        ITimeEntryRepository timeEntryRepository,
        ILogger<GetDashboardOverviewQueryHandler> logger)
    {
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _timeEntryRepository = timeEntryRepository;
        _logger = logger;
    }

    public async Task<Result<DashboardOverviewDto>> Handle(GetDashboardOverviewQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting dashboard overview for organization: {OrganizationId}, user: {UserId}", 
            request.OrganizationId, request.UserId);

        var now = DateTime.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        // Get projects
        var projects = request.OrganizationId.HasValue
            ? await _projectRepository.GetByOrganizationIdAsync(request.OrganizationId.Value, cancellationToken)
            : await _projectRepository.GetAllAsync(cancellationToken);
        var projectList = projects.ToList();

        // Get tasks
        var tasks = request.UserId.HasValue
            ? await _taskRepository.GetByAssignedUserIdAsync(request.UserId.Value, cancellationToken)
            : await _taskRepository.GetAllAsync(cancellationToken);
        var taskList = tasks.ToList();

        // Get time entries
        IEnumerable<Domain.Entities.TimeEntry> timeEntries;
        if (request.UserId.HasValue)
        {
            timeEntries = await _timeEntryRepository.GetByUserIdAsync(request.UserId.Value, cancellationToken);
        }
        else
        {
            timeEntries = await _timeEntryRepository.GetAllAsync(cancellationToken);
        }
        var timeEntryList = timeEntries.ToList();

        // Calculate project statistics
        var projectStats = new ProjectStatisticsDto
        {
            TotalProjects = projectList.Count,
            ActiveProjects = projectList.Count(p => p.Status == ProjectStatus.Active),
            CompletedProjects = projectList.Count(p => p.Status == ProjectStatus.Completed),
            OverdueProjects = projectList.Count(p => p.Status == ProjectStatus.Active && p.DateRange.EndDate < now)
        };

        // Calculate task statistics
        var completedTasks = taskList.Count(t => t.Status == TaskStatus.Done);
        var taskStats = new TaskStatisticsDto
        {
            TotalTasks = taskList.Count,
            CompletedTasks = completedTasks,
            InProgressTasks = taskList.Count(t => t.Status == TaskStatus.InProgress),
            OverdueTasks = taskList.Count(t => t.Status != TaskStatus.Done && t.DueDate.HasValue && t.DueDate.Value < now),
            CompletionRate = taskList.Count > 0 ? (decimal)completedTasks / taskList.Count * 100 : 0
        };

        // Calculate time tracking
        var entriesThisWeek = timeEntryList.Where(te => te.Date >= startOfWeek).ToList();
        var entriesThisMonth = timeEntryList.Where(te => te.Date >= startOfMonth).ToList();
        var daysInMonth = (now - startOfMonth).Days + 1;

        var timeTracking = new TimeTrackingDto
        {
            TotalHoursThisWeek = entriesThisWeek.Sum(te => te.Hours),
            TotalHoursThisMonth = entriesThisMonth.Sum(te => te.Hours),
            BillableHoursThisMonth = entriesThisMonth.Where(te => te.IsBillable).Sum(te => te.Hours),
            AverageHoursPerDay = daysInMonth > 0 ? entriesThisMonth.Sum(te => te.Hours) / daysInMonth : 0
        };

        var overview = new DashboardOverviewDto
        {
            ProjectStatistics = projectStats,
            TaskStatistics = taskStats,
            TimeTracking = timeTracking,
            RecentActivities = new List<RecentActivityDto>() // Can be populated from audit logs
        };

        _logger.LogInformation("Dashboard overview calculated successfully");

        return Result<DashboardOverviewDto>.Success(overview);
    }
}
