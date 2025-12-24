using MediatR;
using Microsoft.Extensions.Logging;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Dashboard.Queries.GetTimeDistribution;

public class GetTimeDistributionQueryHandler : IRequestHandler<GetTimeDistributionQuery, Result<TimeDistributionDto>>
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly ILogger<GetTimeDistributionQueryHandler> _logger;

    public GetTimeDistributionQueryHandler(
        ITimeEntryRepository timeEntryRepository,
        ILogger<GetTimeDistributionQueryHandler> logger)
    {
        _timeEntryRepository = timeEntryRepository;
        _logger = logger;
    }

    public async Task<Result<TimeDistributionDto>> Handle(GetTimeDistributionQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting time distribution for user: {UserId}, project: {ProjectId}", 
            request.UserId, request.ProjectId);

        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // Get time entries based on filters
        IEnumerable<Domain.Entities.TimeEntry> timeEntries;

        if (request.UserId.HasValue)
        {
            timeEntries = await _timeEntryRepository.GetByDateRangeAsync(
                request.UserId.Value, startDate, endDate, cancellationToken);
        }
        else if (request.ProjectId.HasValue)
        {
            timeEntries = await _timeEntryRepository.GetByProjectIdAsync(
                request.ProjectId.Value, cancellationToken);
            timeEntries = timeEntries.Where(te => te.Date >= startDate && te.Date <= endDate);
        }
        else
        {
            timeEntries = await _timeEntryRepository.GetAllAsync(cancellationToken);
            timeEntries = timeEntries.Where(te => te.Date >= startDate && te.Date <= endDate);
        }

        var timeEntryList = timeEntries.ToList();

        // Calculate distributions
        var hoursByProject = timeEntryList
            .GroupBy(te => te.Task.Project.Name)
            .ToDictionary(g => g.Key, g => g.Sum(te => te.Hours));

        var hoursByTask = timeEntryList
            .GroupBy(te => te.Task.Title)
            .OrderByDescending(g => g.Sum(te => te.Hours))
            .Take(10)  // Top 10 tasks
            .ToDictionary(g => g.Key, g => g.Sum(te => te.Hours));

        var hoursByDay = timeEntryList
            .GroupBy(te => te.Date.ToString("yyyy-MM-dd"))
            .ToDictionary(g => g.Key, g => g.Sum(te => te.Hours));

        var billableHours = timeEntryList.Where(te => te.IsBillable).Sum(te => te.Hours);
        var nonBillableHours = timeEntryList.Where(te => !te.IsBillable).Sum(te => te.Hours);

        var distribution = new TimeDistributionDto
        {
            UserId = request.UserId,
            ProjectId = request.ProjectId,
            StartDate = startDate,
            EndDate = endDate,
            TotalHours = timeEntryList.Sum(te => te.Hours),
            HoursByProject = hoursByProject,
            HoursByTask = hoursByTask,
            HoursByDay = hoursByDay,
            BillableHours = billableHours,
            NonBillableHours = nonBillableHours
        };

        _logger.LogInformation("Time distribution calculated: {TotalHours} total hours", 
            distribution.TotalHours);

        return Result<TimeDistributionDto>.Success(distribution);
    }
}
