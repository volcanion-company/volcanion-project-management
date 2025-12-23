using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Sprints.DTOs;

namespace VolcanionPM.Application.Features.Sprints.Queries.GetSprintById;

public class GetSprintByIdQueryHandler : IRequestHandler<GetSprintByIdQuery, Result<SprintDto>>
{
    private readonly ISprintRepository _sprintRepository;
    private readonly IProjectRepository _projectRepository;

    public GetSprintByIdQueryHandler(
        ISprintRepository sprintRepository,
        IProjectRepository projectRepository)
    {
        _sprintRepository = sprintRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Result<SprintDto>> Handle(GetSprintByIdQuery request, CancellationToken cancellationToken)
    {
        var sprint = await _sprintRepository.GetSprintWithTasksAsync(request.Id, cancellationToken);

        if (sprint == null)
        {
            return Result<SprintDto>.Failure("Sprint not found");
        }

        var project = await _projectRepository.GetByIdAsync(sprint.ProjectId, cancellationToken);

        var sprintDto = new SprintDto
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
            ProjectName = project?.Name,
            TaskCount = sprint.Tasks.Count,
            CompletedTaskCount = sprint.GetCompletedTaskCount(),
            CreatedDate = sprint.CreatedAt,
            LastModifiedDate = sprint.UpdatedAt
        };

        return Result<SprintDto>.Success(sprintDto);
    }
}
