using MediatR;
using Microsoft.Extensions.Logging;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using TaskStatus = VolcanionPM.Domain.Enums.TaskStatus;

namespace VolcanionPM.Application.Features.Dashboard.Queries.GetUserProductivity;

public class GetUserProductivityQueryHandler : IRequestHandler<GetUserProductivityQuery, Result<UserProductivityDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly ILogger<GetUserProductivityQueryHandler> _logger;

    public GetUserProductivityQueryHandler(
        IUserRepository userRepository,
        ITaskRepository taskRepository,
        ITimeEntryRepository timeEntryRepository,
        ILogger<GetUserProductivityQueryHandler> logger)
    {
        _userRepository = userRepository;
        _taskRepository = taskRepository;
        _timeEntryRepository = timeEntryRepository;
        _logger = logger;
    }

    public async Task<Result<UserProductivityDto>> Handle(GetUserProductivityQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting productivity data for user: {UserId}", request.UserId);

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result<UserProductivityDto>.Failure("User not found");
        }

        var tasks = await _taskRepository.GetByAssignedUserIdAsync(request.UserId, cancellationToken);
        var taskList = tasks.ToList();

        // Get time entries within date range
        IEnumerable<Domain.Entities.TimeEntry> timeEntries;
        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            timeEntries = await _timeEntryRepository.GetByDateRangeAsync(
                request.UserId, request.StartDate.Value, request.EndDate.Value, cancellationToken);
        }
        else
        {
            timeEntries = await _timeEntryRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        }

        var timeEntryList = timeEntries.ToList();
        var now = DateTime.UtcNow;

        var completedTasks = taskList.Count(t => t.Status == TaskStatus.Done);
        var assignedTasks = taskList.Count;

        // Calculate average completion time
        var completedWithDates = taskList
            .Where(t => t.Status == TaskStatus.Done && t.UpdatedAt.HasValue)
            .ToList();
        
        var avgCompletionDays = completedWithDates.Any()
            ? completedWithDates.Average(t => (t.UpdatedAt!.Value - t.CreatedAt).TotalDays)
            : 0;

        // Group hours by project
        var hoursByProject = timeEntryList
            .GroupBy(te => te.Task.Project.Name)
            .ToDictionary(g => g.Key, g => g.Sum(te => te.Hours));

        // Get top tasks by hours logged
        var topTasks = timeEntryList
            .GroupBy(te => te.Task)
            .Select(g => new TopTaskDto
            {
                TaskId = g.Key.Id,
                TaskTitle = g.Key.Title,
                ProjectName = g.Key.Project.Name,
                HoursLogged = g.Sum(te => te.Hours),
                Status = g.Key.Status.ToString()
            })
            .OrderByDescending(t => t.HoursLogged)
            .Take(5)
            .ToList();

        var productivity = new UserProductivityDto
        {
            UserId = user.Id,
            UserName = user.GetFullName(),
            AssignedTasks = assignedTasks,
            CompletedTasks = completedTasks,
            OverdueTasks = taskList.Count(t => t.Status != TaskStatus.Done && t.DueDate.HasValue && t.DueDate.Value < now),
            TotalHoursLogged = timeEntryList.Sum(te => te.Hours),
            CompletionRate = assignedTasks > 0 ? (decimal)completedTasks / assignedTasks * 100 : 0,
            AverageTaskCompletionDays = (decimal)avgCompletionDays,
            HoursByProject = hoursByProject,
            TopTasks = topTasks
        };

        _logger.LogInformation("User productivity calculated: {CompletedTasks}/{AssignedTasks} tasks, {TotalHours} hours", 
            productivity.CompletedTasks, productivity.AssignedTasks, productivity.TotalHoursLogged);

        return Result<UserProductivityDto>.Success(productivity);
    }
}
