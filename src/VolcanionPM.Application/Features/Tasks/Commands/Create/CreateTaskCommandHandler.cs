using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Tasks;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Tasks.Commands.Create;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<TaskDto>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskCommandHandler(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TaskDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        // Verify project exists
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
        {
            return Result<TaskDto>.Failure($"Project with ID {request.ProjectId} not found");
        }

        // Parse enums
        if (!Enum.TryParse<TaskType>(request.Type, true, out var taskType))
        {
            return Result<TaskDto>.Failure($"Invalid task type: {request.Type}");
        }

        if (!Enum.TryParse<TaskPriority>(request.Priority, true, out var priority))
        {
            return Result<TaskDto>.Failure($"Invalid priority: {request.Priority}");
        }

        // Generate task code
        var existingTasks = await _taskRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);
        var taskNumber = existingTasks.Count() + 1;
        var taskCode = $"{project.Code}-{taskNumber:D3}";

        // Create task
        var task = ProjectTask.Create(
            request.Title,
            taskCode,
            request.ProjectId,
            taskType,
            priority,
            request.EstimatedHours,
            request.Description,
            request.AssignedToId,
            null, // sprintId
            null, // parentTaskId
            request.DueDate,
            request.StoryPoints,
            request.CreatedBy
        );

        await _taskRepository.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
