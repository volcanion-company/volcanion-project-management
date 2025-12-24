using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionPM.Application.Features.Dashboard.Queries.GetDashboardOverview;
using VolcanionPM.Application.Features.Dashboard.Queries.GetProjectStatistics;
using VolcanionPM.Application.Features.Dashboard.Queries.GetTaskStatistics;
using VolcanionPM.Application.Features.Dashboard.Queries.GetUserProductivity;
using VolcanionPM.Application.Features.Dashboard.Queries.GetSprintBurndown;
using VolcanionPM.Application.Features.Dashboard.Queries.GetTeamVelocity;
using VolcanionPM.Application.Features.Dashboard.Queries.GetTimeDistribution;

namespace VolcanionPM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IMediator mediator, ILogger<DashboardController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard overview with key metrics
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(DashboardOverviewDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview(
        [FromQuery] Guid? organizationId,
        [FromQuery] Guid? userId,
        CancellationToken cancellationToken)
    {
        var query = new GetDashboardOverviewQuery
        {
            OrganizationId = organizationId,
            UserId = userId
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get project statistics
    /// </summary>
    [HttpGet("project-stats")]
    [ProducesResponseType(typeof(Application.Features.Dashboard.Queries.GetProjectStatistics.ProjectStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectStatistics(
        [FromQuery] Guid? organizationId,
        CancellationToken cancellationToken)
    {
        var query = new GetProjectStatisticsQuery
        {
            OrganizationId = organizationId
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get task statistics
    /// </summary>
    [HttpGet("task-stats")]
    [ProducesResponseType(typeof(Application.Features.Dashboard.Queries.GetTaskStatistics.TaskStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaskStatistics(
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? userId,
        CancellationToken cancellationToken)
    {
        var query = new GetTaskStatisticsQuery
        {
            ProjectId = projectId,
            UserId = userId
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get user productivity metrics
    /// </summary>
    [HttpGet("user-productivity/{userId:guid}")]
    [ProducesResponseType(typeof(UserProductivityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserProductivity(
        Guid userId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var query = new GetUserProductivityQuery
        {
            UserId = userId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get sprint burndown chart data
    /// </summary>
    [HttpGet("burndown/{sprintId:guid}")]
    [ProducesResponseType(typeof(SprintBurndownDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSprintBurndown(
        Guid sprintId,
        CancellationToken cancellationToken)
    {
        var query = new GetSprintBurndownQuery { SprintId = sprintId };
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get team velocity chart data
    /// </summary>
    [HttpGet("velocity")]
    [ProducesResponseType(typeof(TeamVelocityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeamVelocity(
        [FromQuery] Guid projectId,
        [FromQuery] int? lastNSprints,
        CancellationToken cancellationToken)
    {
        var query = new GetTeamVelocityQuery
        {
            ProjectId = projectId,
            LastNSprints = lastNSprints ?? 5
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get time distribution chart data
    /// </summary>
    [HttpGet("time-distribution")]
    [ProducesResponseType(typeof(TimeDistributionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimeDistribution(
        [FromQuery] Guid? userId,
        [FromQuery] Guid? projectId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var query = new GetTimeDistributionQuery
        {
            UserId = userId,
            ProjectId = projectId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }
}
