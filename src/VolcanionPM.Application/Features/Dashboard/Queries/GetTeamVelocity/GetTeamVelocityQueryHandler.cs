using MediatR;
using Microsoft.Extensions.Logging;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Enums;
using TaskStatus = VolcanionPM.Domain.Enums.TaskStatus;

namespace VolcanionPM.Application.Features.Dashboard.Queries.GetTeamVelocity;

public class GetTeamVelocityQueryHandler : IRequestHandler<GetTeamVelocityQuery, Result<TeamVelocityDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ISprintRepository _sprintRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<GetTeamVelocityQueryHandler> _logger;

    public GetTeamVelocityQueryHandler(
        IProjectRepository projectRepository,
        ISprintRepository sprintRepository,
        ITaskRepository taskRepository,
        ILogger<GetTeamVelocityQueryHandler> logger)
    {
        _projectRepository = projectRepository;
        _sprintRepository = sprintRepository;
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<Result<TeamVelocityDto>> Handle(GetTeamVelocityQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting team velocity for project: {ProjectId}", request.ProjectId);

        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
        {
            return Result<TeamVelocityDto>.Failure("Project not found");
        }

        var sprints = await _sprintRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);
        var completedSprints = sprints
            .Where(s => s.Status == SprintStatus.Completed)
            .OrderByDescending(s => s.DateRange.EndDate)
            .Take(request.LastNSprints ?? 5)
            .ToList();

        var sprintVelocities = new List<SprintVelocityDto>();

        foreach (var sprint in completedSprints)
        {
            var tasks = await _taskRepository.GetBySprintIdAsync(sprint.Id, cancellationToken);
            var taskList = tasks.ToList();

            var plannedPoints = taskList.Sum(t => t.StoryPoints ?? 0);
            var completedPoints = taskList
                .Where(t => t.Status == TaskStatus.Done)
                .Sum(t => t.StoryPoints ?? 0);

            sprintVelocities.Add(new SprintVelocityDto
            {
                SprintId = sprint.Id,
                SprintName = sprint.Name,
                StartDate = sprint.DateRange.StartDate,
                EndDate = sprint.DateRange.EndDate,
                PlannedStoryPoints = plannedPoints,
                CompletedStoryPoints = completedPoints,
                CompletionRate = plannedPoints > 0 ? (decimal)completedPoints / plannedPoints * 100 : 0
            });
        }

        var averageVelocity = sprintVelocities.Any()
            ? (decimal)sprintVelocities.Average(sv => sv.CompletedStoryPoints)
            : 0;

        var velocity = new TeamVelocityDto
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            AverageVelocity = averageVelocity,
            SprintVelocities = sprintVelocities.OrderBy(sv => sv.StartDate).ToList()
        };

        _logger.LogInformation("Team velocity calculated: {AverageVelocity} story points per sprint", 
            averageVelocity);

        return Result<TeamVelocityDto>.Success(velocity);
    }
}
