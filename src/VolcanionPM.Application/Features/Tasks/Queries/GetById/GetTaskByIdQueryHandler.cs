using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Tasks;

namespace VolcanionPM.Application.Features.Tasks.Queries.GetById;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, Result<TaskDto>>
{
    private readonly ITaskRepository _taskRepository;

    public GetTaskByIdQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<TaskDto>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.Id, cancellationToken);

        if (task == null)
        {
            return Result<TaskDto>.Failure($"Task with ID {request.Id} not found");
        }

        var dto = new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Code = task.Code,
            Description = task.Description,
            Type = task.Type.ToString(),
            Status = task.Status.ToString(),
            Priority = task.Priority.ToString(),
            StoryPoints = task.StoryPoints,
            EstimatedHours = task.EstimatedHours,
            ActualHours = task.ActualHours,
            DueDate = task.DueDate,
            AssignedToId = task.AssignedToId,
            ProjectId = task.ProjectId,
            SprintId = task.SprintId,
            CreatedAt = task.CreatedAt
        };

        return Result<TaskDto>.Success(dto);
    }
}
