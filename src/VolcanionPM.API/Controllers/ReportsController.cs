using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionPM.Application.Features.Reporting.Queries.GetProjectProgressReport;
using VolcanionPM.Application.Features.Reporting.Queries.GetResourceUtilizationReport;
using VolcanionPM.Application.Features.Reporting.Queries.GetTimeCostReport;

namespace VolcanionPM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IMediator mediator, ILogger<ReportsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive project progress report
    /// </summary>
    [HttpGet("project-progress/{projectId:guid}")]
    [ProducesResponseType(typeof(ProjectProgressReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectProgressReport(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var query = new GetProjectProgressReportQuery { ProjectId = projectId };
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get resource utilization report
    /// </summary>
    [HttpGet("resource-utilization")]
    [ProducesResponseType(typeof(ResourceUtilizationReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResourceUtilizationReport(
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? organizationId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var query = new GetResourceUtilizationReportQuery
        {
            ProjectId = projectId,
            OrganizationId = organizationId,
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

    /// <summary>
    /// Get time and cost analysis report
    /// </summary>
    [HttpGet("time-cost")]
    [ProducesResponseType(typeof(TimeCostReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimeCostReport(
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? organizationId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var query = new GetTimeCostReportQuery
        {
            ProjectId = projectId,
            OrganizationId = organizationId,
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
