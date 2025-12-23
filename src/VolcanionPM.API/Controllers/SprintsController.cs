using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionPM.Application.Features.Sprints.Commands.CompleteSprint;
using VolcanionPM.Application.Features.Sprints.Commands.Create;
using VolcanionPM.Application.Features.Sprints.Commands.Delete;
using VolcanionPM.Application.Features.Sprints.Commands.StartSprint;
using VolcanionPM.Application.Features.Sprints.Commands.Update;
using VolcanionPM.Application.Features.Sprints.Queries.GetSprintById;
using VolcanionPM.Application.Features.Sprints.Queries.GetSprintsByProject;

namespace VolcanionPM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SprintsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SprintsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetByProject(Guid projectId)
    {
        var query = new GetSprintsByProjectQuery(projectId);
        var result = await _mediator.Send(query);

        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetSprintByIdQuery(id);
        var result = await _mediator.Send(query);

        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> Create([FromBody] CreateSprintCommand command)
    {
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSprintCommand command)
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
        var command = new DeleteSprintCommand(id);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    [HttpPost("{id}/start")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> StartSprint(Guid id)
    {
        var command = new StartSprintCommand(id);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> CompleteSprint(Guid id)
    {
        var command = new CompleteSprintCommand(id);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
