using MediatR;
using Microsoft.Extensions.Logging;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Dashboard.Queries.GetProjectStatistics;

public class GetProjectStatisticsQueryHandler : IRequestHandler<GetProjectStatisticsQuery, Result<ProjectStatisticsDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<GetProjectStatisticsQueryHandler> _logger;

    public GetProjectStatisticsQueryHandler(
        IProjectRepository projectRepository,
        ILogger<GetProjectStatisticsQueryHandler> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<Result<ProjectStatisticsDto>> Handle(GetProjectStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting project statistics for organization: {OrganizationId}", request.OrganizationId);

        var projects = request.OrganizationId.HasValue
            ? await _projectRepository.GetByOrganizationIdAsync(request.OrganizationId.Value, cancellationToken)
            : await _projectRepository.GetAllAsync(cancellationToken);

        var projectList = projects.ToList();
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var statistics = new ProjectStatisticsDto
        {
            TotalProjects = projectList.Count,
            ActiveProjects = projectList.Count(p => p.Status == ProjectStatus.Active),
            CompletedProjects = projectList.Count(p => p.Status == ProjectStatus.Completed),
            OnHoldProjects = projectList.Count(p => p.Status == ProjectStatus.OnHold),
            CancelledProjects = projectList.Count(p => p.Status == ProjectStatus.Cancelled),
            AverageProgressPercentage = projectList.Any() ? projectList.Average(p => p.ProgressPercentage) : 0,
            OverdueProjects = projectList.Count(p => p.Status == ProjectStatus.Active && p.DateRange.EndDate < now),
            ProjectsStartingThisMonth = projectList.Count(p => p.DateRange.StartDate >= startOfMonth && p.DateRange.StartDate <= endOfMonth),
            ProjectsEndingThisMonth = projectList.Count(p => p.DateRange.EndDate >= startOfMonth && p.DateRange.EndDate <= endOfMonth),
            ProjectsByPriority = projectList
                .GroupBy(p => p.Priority.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            ProjectsByStatus = projectList
                .GroupBy(p => p.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count())
        };

        _logger.LogInformation("Project statistics calculated: {TotalProjects} total, {ActiveProjects} active", 
            statistics.TotalProjects, statistics.ActiveProjects);

        return Result<ProjectStatisticsDto>.Success(statistics);
    }
}
