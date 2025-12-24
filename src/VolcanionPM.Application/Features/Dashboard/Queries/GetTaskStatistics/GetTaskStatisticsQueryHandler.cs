using MediatR;
using Microsoft.Extensions.Logging;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using TaskStatus = VolcanionPM.Domain.Enums.TaskStatus;

namespace VolcanionPM.Application.Features.Dashboard.Queries.GetTaskStatistics;

public class GetTaskStatisticsQueryHandler : IRequestHandler<GetTaskStatisticsQuery, Result<TaskStatisticsDto>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<GetTaskStatisticsQueryHandler> _logger;

    public GetTaskStatisticsQueryHandler(
        ITaskRepository taskRepository,
        ILogger<GetTaskStatisticsQueryHandler> logger)
    {
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<Result<TaskStatisticsDto>> Handle(GetTaskStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting task statistics for project: {ProjectId}, user: {UserId}", 
            request.ProjectId, request.UserId);

        IEnumerable<Domain.Entities.ProjectTask> tasks;

        if (request.ProjectId.HasValue)
        {
            tasks = await _taskRepository.GetByProjectIdAsync(request.ProjectId.Value, cancellationToken);
        }
        else if (request.UserId.HasValue)
        {
            tasks = await _taskRepository.GetByAssignedUserIdAsync(request.UserId.Value, cancellationToken);
        }
        else
        {
            tasks = await _taskRepository.GetAllAsync(cancellationToken);
        }

        var taskList = tasks.ToList();
        var now = DateTime.UtcNow;

        var completedTasks = taskList.Count(t => t.Status == TaskStatus.Done);
        var totalTasks = taskList.Count;

        var statistics = new TaskStatisticsDto
        {
            TotalTasks = totalTasks,
            ToDoTasks = taskList.Count(t => t.Status == TaskStatus.ToDo),
            InProgressTasks = taskList.Count(t => t.Status == TaskStatus.InProgress),
            CompletedTasks = completedTasks,
            BlockedTasks = taskList.Count(t => t.Status == TaskStatus.Blocked),
            OverdueTasks = taskList.Count(t => t.Status != TaskStatus.Done && t.DueDate.HasValue && t.DueDate.Value < now),
            CompletionRate = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0,
            AverageEstimatedHours = taskList.Any(t => t.EstimatedHours > 0) 
                ? taskList.Where(t => t.EstimatedHours > 0).Average(t => t.EstimatedHours) : 0,
            AverageActualHours = taskList.Any(t => t.ActualHours > 0) 
                ? taskList.Where(t => t.ActualHours > 0).Average(t => t.ActualHours) : 0,
            UnassignedTasks = taskList.Count(t => !t.AssignedToId.HasValue),
            TasksByPriority = taskList
                .GroupBy(t => t.Priority.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            TasksByStatus = taskList
                .GroupBy(t => t.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            TasksByType = taskList
                .GroupBy(t => t.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count())
        };

        _logger.LogInformation("Task statistics calculated: {TotalTasks} total, {CompletionRate}% completion rate", 
            statistics.TotalTasks, statistics.CompletionRate);

        return Result<TaskStatisticsDto>.Success(statistics);
    }
}
