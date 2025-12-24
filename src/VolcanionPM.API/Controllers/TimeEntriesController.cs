using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionPM.Application.Features.TimeEntries.Commands.Create;
using VolcanionPM.Application.Features.TimeEntries.Commands.Delete;
using VolcanionPM.Application.Features.TimeEntries.Commands.Update;
using VolcanionPM.Application.Features.TimeEntries.Queries.GetTimeEntriesByTask;
using VolcanionPM.Application.Features.TimeEntries.Queries.GetTimeEntriesByUser;
using VolcanionPM.Application.Features.TimeEntries.Queries.GetTimeEntryById;

namespace VolcanionPM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TimeEntriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TimeEntriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetTimeEntryByIdQuery(id);
        var result = await _mediator.Send(query);

        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    /// <summary>
    /// Get time entries by user with pagination, filtering, and sorting
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(
        Guid userId,
        [FromQuery] Guid? taskId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? type,
        [FromQuery] bool? isBillable,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTimeEntriesByUserQuery
        {
            UserId = userId,
            TaskId = taskId,
            StartDate = startDate,
            EndDate = endDate,
            Type = type,
            IsBillable = isBillable,
            SortBy = sortBy ?? "date",
            SortOrder = sortOrder ?? "desc",
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpGet("task/{taskId}")]
    public async Task<IActionResult> GetByTask(Guid taskId)
    {
        var query = new GetTimeEntriesByTaskQuery(taskId);
        var result = await _mediator.Send(query);

        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTimeEntryCommand command)
    {
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTimeEntryCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }

        var result = await _mediator.Send(command);

        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteTimeEntryCommand(id);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}
