using MediatR;
using Microsoft.Extensions.Logging;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Reporting.Queries.GetTimeCostReport;

public class GetTimeCostReportQueryHandler : IRequestHandler<GetTimeCostReportQuery, Result<TimeCostReportDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly ILogger<GetTimeCostReportQueryHandler> _logger;

    private const decimal DEFAULT_HOURLY_RATE = 100m;
    private const decimal DEFAULT_BILLABLE_RATE = 150m;

    public GetTimeCostReportQueryHandler(
        IProjectRepository projectRepository,
        ITaskRepository taskRepository,
        ITimeEntryRepository timeEntryRepository,
        ILogger<GetTimeCostReportQueryHandler> logger)
    {
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _timeEntryRepository = timeEntryRepository;
        _logger = logger;
    }

    public async Task<Result<TimeCostReportDto>> Handle(GetTimeCostReportQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating time & cost report for project: {ProjectId}, organization: {OrgId}", 
            request.ProjectId, request.OrganizationId);

        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        decimal budget = 0;
        string? projectName = null;

        // Get project-specific data if ProjectId provided
        if (request.ProjectId.HasValue)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId.Value, cancellationToken);
            if (project == null)
            {
                return Result<TimeCostReportDto>.Failure("Project not found");
            }
            budget = project.Budget.Amount;
            projectName = project.Name;
        }

        // Get tasks
        IEnumerable<Domain.Entities.ProjectTask> tasks;
        if (request.ProjectId.HasValue)
        {
            tasks = await _taskRepository.GetByProjectIdAsync(request.ProjectId.Value, cancellationToken);
        }
        else
        {
            tasks = await _taskRepository.GetAllAsync(cancellationToken);
        }
        var taskList = tasks.ToList();

        // Get time entries
        IEnumerable<Domain.Entities.TimeEntry> timeEntries;
        if (request.ProjectId.HasValue)
        {
            timeEntries = await _timeEntryRepository.GetByProjectIdAsync(request.ProjectId.Value, cancellationToken);
            timeEntries = timeEntries.Where(te => te.Date >= startDate && te.Date <= endDate);
        }
        else
        {
            timeEntries = await _timeEntryRepository.GetAllAsync(cancellationToken);
            timeEntries = timeEntries.Where(te => te.Date >= startDate && te.Date <= endDate);
        }
        var timeEntryList = timeEntries.ToList();

        // Calculate hours
        var totalHoursLogged = timeEntryList.Sum(te => te.Hours);
        var totalEstimatedHours = taskList.Sum(t => t.EstimatedHours);
        var timeVariance = totalHoursLogged - totalEstimatedHours;
        var timeVariancePercentage = totalEstimatedHours > 0 
            ? (timeVariance / totalEstimatedHours) * 100 
            : 0;

        // Billable vs Non-billable
        var billableHours = timeEntryList.Where(te => te.IsBillable).Sum(te => te.Hours);
        var nonBillableHours = timeEntryList.Where(te => !te.IsBillable).Sum(te => te.Hours);
        var billablePercentage = totalHoursLogged > 0 
            ? (billableHours / totalHoursLogged) * 100 
            : 0;

        // Cost calculations
        var actualCost = totalHoursLogged * DEFAULT_HOURLY_RATE;
        var estimatedCost = totalEstimatedHours * DEFAULT_HOURLY_RATE;
        var costVariance = actualCost - estimatedCost;
        var costVariancePercentage = estimatedCost > 0 
            ? (costVariance / estimatedCost) * 100 
            : 0;

        // Revenue calculations
        var billableRevenue = billableHours * DEFAULT_BILLABLE_RATE;
        var totalRevenue = billableRevenue; // Simplified - could include other revenue sources
        var profit = totalRevenue - actualCost;
        var profitMargin = totalRevenue > 0 ? (profit / totalRevenue) * 100 : 0;

        // Cost by resource
        var costByResource = timeEntryList
            .GroupBy(te => $"{te.User.FirstName} {te.User.LastName}")
            .ToDictionary(g => g.Key, g => g.Sum(te => te.Hours * DEFAULT_HOURLY_RATE));

        var hoursByResource = timeEntryList
            .GroupBy(te => $"{te.User.FirstName} {te.User.LastName}")
            .ToDictionary(g => g.Key, g => g.Sum(te => te.Hours));

        // Efficiency metrics
        var costPerHour = totalHoursLogged > 0 ? actualCost / totalHoursLogged : 0;
        var revenuePerHour = totalHoursLogged > 0 ? totalRevenue / totalHoursLogged : 0;
        var efficiencyRatio = actualCost > 0 ? totalRevenue / actualCost : 0;

        // Weekly trends
        var weeklyCostTrend = CalculateWeeklyCostTrend(timeEntryList, startDate, endDate);
        var weeklyTimeTrend = CalculateWeeklyTimeTrend(timeEntryList, startDate, endDate);

        // Warnings and insights
        var warnings = new List<string>();
        var insights = new List<string>();

        if (budget > 0 && actualCost > budget)
            warnings.Add($"Project is over budget by {actualCost - budget:C} ({costVariancePercentage:F1}%)");
        
        if (budget > 0 && actualCost > budget * 0.8m)
            warnings.Add($"Project has used {(actualCost / budget) * 100:F1}% of budget");
        
        if (timeVariancePercentage > 20)
            warnings.Add($"Time variance is {timeVariancePercentage:F1}% - tasks taking longer than estimated");

        if (billablePercentage < 70)
            insights.Add($"Only {billablePercentage:F1}% of hours are billable - consider increasing billable work");
        
        if (profitMargin < 20)
            insights.Add($"Profit margin is {profitMargin:F1}% - below recommended 20%+ target");
        
        if (efficiencyRatio < 1.5m)
            insights.Add($"Efficiency ratio is {efficiencyRatio:F2} - aim for 1.5+ for healthy margins");

        var report = new TimeCostReportDto
        {
            ProjectId = request.ProjectId,
            ProjectName = projectName,
            StartDate = startDate,
            EndDate = endDate,
            
            Budget = budget,
            EstimatedCost = estimatedCost,
            ActualCost = actualCost,
            CostVariance = costVariance,
            CostVariancePercentage = costVariancePercentage,
            IsOverBudget = budget > 0 && actualCost > budget,
            
            TotalHoursLogged = totalHoursLogged,
            TotalEstimatedHours = totalEstimatedHours,
            TimeVariance = timeVariance,
            TimeVariancePercentage = timeVariancePercentage,
            
            BillableHours = billableHours,
            NonBillableHours = nonBillableHours,
            BillablePercentage = billablePercentage,
            BillableRevenue = billableRevenue,
            
            CostByResource = costByResource,
            HoursByResource = hoursByResource,
            
            TotalRevenue = totalRevenue,
            Profit = profit,
            ProfitMargin = profitMargin,
            
            CostPerHour = costPerHour,
            RevenuePerHour = revenuePerHour,
            EfficiencyRatio = efficiencyRatio,
            
            WeeklyCostTrend = weeklyCostTrend,
            WeeklyTimeTrend = weeklyTimeTrend,
            
            BudgetWarnings = warnings,
            CostInsights = insights
        };

        _logger.LogInformation("Time & cost report generated: {ActualCost:C} cost, {TotalHours:F1} hours, {ProfitMargin:F1}% margin", 
            report.ActualCost, report.TotalHoursLogged, report.ProfitMargin);

        return Result<TimeCostReportDto>.Success(report);
    }

    private List<CostTrendDto> CalculateWeeklyCostTrend(
        List<Domain.Entities.TimeEntry> timeEntries,
        DateTime startDate,
        DateTime endDate)
    {
        var trends = new List<CostTrendDto>();
        var currentWeekStart = startDate.Date.AddDays(-(int)startDate.DayOfWeek);

        while (currentWeekStart <= endDate)
        {
            var weekEnd = currentWeekStart.AddDays(7);
            var weekEntries = timeEntries.Where(te => te.Date >= currentWeekStart && te.Date < weekEnd);
            var actualCost = weekEntries.Sum(te => te.Hours * DEFAULT_HOURLY_RATE);
            
            // Simplified planned cost - could be based on estimated hours
            var plannedCost = actualCost * 0.9m; // Assume 10% under budget as baseline

            trends.Add(new CostTrendDto
            {
                WeekStart = currentWeekStart,
                WeekEnd = weekEnd.AddDays(-1),
                ActualCost = actualCost,
                PlannedCost = plannedCost
            });

            currentWeekStart = weekEnd;
        }

        return trends;
    }

    private List<TimeTrendDto> CalculateWeeklyTimeTrend(
        List<Domain.Entities.TimeEntry> timeEntries,
        DateTime startDate,
        DateTime endDate)
    {
        var trends = new List<TimeTrendDto>();
        var currentWeekStart = startDate.Date.AddDays(-(int)startDate.DayOfWeek);

        while (currentWeekStart <= endDate)
        {
            var weekEnd = currentWeekStart.AddDays(7);
            var weekEntries = timeEntries.Where(te => te.Date >= currentWeekStart && te.Date < weekEnd);
            var hoursLogged = weekEntries.Sum(te => te.Hours);
            
            // Simplified planned hours - could be based on sprint planning
            var plannedHours = hoursLogged * 0.95m; // Assume slightly under planned as baseline

            trends.Add(new TimeTrendDto
            {
                WeekStart = currentWeekStart,
                WeekEnd = weekEnd.AddDays(-1),
                HoursLogged = hoursLogged,
                PlannedHours = plannedHours
            });

            currentWeekStart = weekEnd;
        }

        return trends;
    }
}
