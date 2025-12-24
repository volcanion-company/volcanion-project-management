using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionPM.Application.Features.Risks.Commands.Create;
using VolcanionPM.Application.Features.Risks.Commands.Delete;
using VolcanionPM.Application.Features.Risks.Commands.Update;
using VolcanionPM.Application.Features.Risks.Queries.GetRiskById;
using VolcanionPM.Application.Features.Risks.Queries.GetRisksByProject;

namespace VolcanionPM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RisksController : ControllerBase
{
    private readonly IMediator _mediator;

    public RisksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetRiskByIdQuery(id);
        var result = await _mediator.Send(query);

        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    /// <summary>
    /// Get risks by project with pagination, filtering, and sorting
    /// </summary>
    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetByProject(
        Guid projectId,
        [FromQuery] string? level,
        [FromQuery] string? status,
        [FromQuery] string? searchTerm,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRisksByProjectQuery
        {
            ProjectId = projectId,
            Level = level,
            Status = status,
            SearchTerm = searchTerm,
            SortBy = sortBy ?? "riskscore",
            SortOrder = sortOrder ?? "desc",
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> Create([FromBody] CreateRiskCommand command)
    {
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRiskCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }

        var result = await _mediator.Send(command);

        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteRiskCommand(id);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}
