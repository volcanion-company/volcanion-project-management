using MediatR;
using Microsoft.Extensions.Logging;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using TaskStatus = VolcanionPM.Domain.Enums.TaskStatus;

namespace VolcanionPM.Application.Features.Dashboard.Queries.GetSprintBurndown;

public class GetSprintBurndownQueryHandler : IRequestHandler<GetSprintBurndownQuery, Result<SprintBurndownDto>>
{
    private readonly ISprintRepository _sprintRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<GetSprintBurndownQueryHandler> _logger;

    public GetSprintBurndownQueryHandler(
        ISprintRepository sprintRepository,
        ITaskRepository taskRepository,
        ILogger<GetSprintBurndownQueryHandler> logger)
    {
        _sprintRepository = sprintRepository;
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<Result<SprintBurndownDto>> Handle(GetSprintBurndownQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting burndown chart data for sprint: {SprintId}", request.SprintId);

        var sprint = await _sprintRepository.GetByIdAsync(request.SprintId, cancellationToken);
        if (sprint == null)
        {
            return Result<SprintBurndownDto>.Failure("Sprint not found");
        }

        var tasks = await _taskRepository.GetBySprintIdAsync(request.SprintId, cancellationToken);
        var taskList = tasks.ToList();

        var totalStoryPoints = taskList.Sum(t => t.StoryPoints ?? 0);
        var sprintDays = (sprint.DateRange.EndDate - sprint.DateRange.StartDate).Days + 1;

        // Calculate ideal burndown (linear)
        var idealBurndown = new List<BurndownDataPoint>();
        var dailyBurnRate = totalStoryPoints / (double)sprintDays;
        
        for (int i = 0; i <= sprintDays; i++)
        {
            var date = sprint.DateRange.StartDate.AddDays(i);
            var remaining = Math.Max(0, totalStoryPoints - (int)(dailyBurnRate * i));
            
            idealBurndown.Add(new BurndownDataPoint
            {
                Date = date,
                RemainingStoryPoints = remaining
            });
        }

        // Calculate actual burndown (based on task completion dates)
        var actualBurndown = new List<BurndownDataPoint>();
        var remainingPoints = totalStoryPoints;

        for (int i = 0; i <= sprintDays; i++)
        {
            var date = sprint.DateRange.StartDate.AddDays(i);
            
            // Calculate points completed up to this date
            var completedPoints = taskList
                .Where(t => t.Status == TaskStatus.Done && t.UpdatedAt.HasValue && t.UpdatedAt.Value.Date <= date)
                .Sum(t => t.StoryPoints ?? 0);

            actualBurndown.Add(new BurndownDataPoint
            {
                Date = date,
                RemainingStoryPoints = Math.Max(0, totalStoryPoints - completedPoints)
            });
        }

        var burndown = new SprintBurndownDto
        {
            SprintId = sprint.Id,
            SprintName = sprint.Name,
            StartDate = sprint.DateRange.StartDate,
            EndDate = sprint.DateRange.EndDate,
            TotalStoryPoints = totalStoryPoints,
            IdealBurndown = idealBurndown,
            ActualBurndown = actualBurndown
        };

        _logger.LogInformation("Burndown chart calculated for sprint {SprintName}: {TotalPoints} total story points", 
            sprint.Name, totalStoryPoints);

        return Result<SprintBurndownDto>.Success(burndown);
    }
}
