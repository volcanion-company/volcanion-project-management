using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Sprints.DTOs;

namespace VolcanionPM.Application.Features.Sprints.Queries.GetSprintsByProject;

public class GetSprintsByProjectQueryHandler : IRequestHandler<GetSprintsByProjectQuery, Result<List<SprintDto>>>
{
    private readonly ISprintRepository _sprintRepository;
    private readonly IProjectRepository _projectRepository;

    public GetSprintsByProjectQueryHandler(
        ISprintRepository sprintRepository,
        IProjectRepository projectRepository)
    {
        _sprintRepository = sprintRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Result<List<SprintDto>>> Handle(GetSprintsByProjectQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
        {
            return Result<List<SprintDto>>.Failure("Project not found");
        }

        var sprints = await _sprintRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);

        var sprintDtos = sprints.Select(sprint => new SprintDto
        {
            Id = sprint.Id,
            Name = sprint.Name,
            Goal = sprint.Goal,
            SprintNumber = sprint.SprintNumber,
            StartDate = sprint.DateRange.StartDate,
            EndDate = sprint.DateRange.EndDate,
            Status = sprint.Status,
            TotalStoryPoints = sprint.TotalStoryPoints,
            CompletedStoryPoints = sprint.CompletedStoryPoints,
            ProjectId = sprint.ProjectId,
            ProjectName = project.Name,
            TaskCount = sprint.Tasks.Count,
            CompletedTaskCount = sprint.GetCompletedTaskCount(),
            CreatedDate = sprint.CreatedAt,
            LastModifiedDate = sprint.UpdatedAt
        }).OrderBy(s => s.SprintNumber).ToList();

        return Result<List<SprintDto>>.Success(sprintDtos);
    }
}
