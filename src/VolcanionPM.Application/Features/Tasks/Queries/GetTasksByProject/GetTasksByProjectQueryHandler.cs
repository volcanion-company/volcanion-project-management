using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Tasks;

namespace VolcanionPM.Application.Features.Tasks.Queries.GetTasksByProject;

public class GetTasksByProjectQueryHandler : IRequestHandler<GetTasksByProjectQuery, Result<List<TaskDto>>>
{
    private readonly ITaskRepository _taskRepository;

    public GetTasksByProjectQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<List<TaskDto>>> Handle(GetTasksByProjectQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _taskRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);

        var dtos = tasks.Select(task => new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Code = task.Code,
            Description = task.Description,
            Type = task.Type.ToString(),
            Priority = task.Priority.ToString(),
            Status = task.Status.ToString(),
            EstimatedHours = task.EstimatedHours,
            ActualHours = task.ActualHours,
            StoryPoints = task.StoryPoints,
            DueDate = task.DueDate,
            AssignedToId = task.AssignedToId,
            ProjectId = task.ProjectId,
            CreatedAt = task.CreatedAt
        }).ToList();

        return Result<List<TaskDto>>.Success(dtos);
    }
}
