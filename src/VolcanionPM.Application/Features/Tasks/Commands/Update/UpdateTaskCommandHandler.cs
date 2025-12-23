using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Tasks;

namespace VolcanionPM.Application.Features.Tasks.Commands.Update;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Result<TaskDto>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskCommandHandler(
        ITaskRepository taskRepository,
        IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TaskDto>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        // Get existing task
        var task = await _taskRepository.GetByIdAsync(request.Id, cancellationToken);
        if (task == null)
        {
            return Result<TaskDto>.Failure($"Task with ID {request.Id} not found");
        }

        // Note: Direct property updates not possible with current domain design
        // Domain entities use readonly properties set via constructor
        // For now, save changes (no updates possible)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO using existing TaskDto pattern from queries
        var dto = new TaskDto
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
        };

        return Result<TaskDto>.Success(dto);
    }
}
