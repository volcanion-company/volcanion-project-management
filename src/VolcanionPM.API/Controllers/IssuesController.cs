using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionPM.Application.Features.Issues.Commands.ChangeStatus;
using VolcanionPM.Application.Features.Issues.Commands.Create;
using VolcanionPM.Application.Features.Issues.Commands.Delete;
using VolcanionPM.Application.Features.Issues.Commands.Resolve;
using VolcanionPM.Application.Features.Issues.Commands.Update;
using VolcanionPM.Application.Features.Issues.Queries.GetIssueById;
using VolcanionPM.Application.Features.Issues.Queries.GetIssuesByProject;

namespace VolcanionPM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class IssuesController : ControllerBase
{
    private readonly IMediator _mediator;

    public IssuesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetIssueByIdQuery(id);
        var result = await _mediator.Send(query);

        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetByProject(Guid projectId)
    {
        var query = new GetIssuesByProjectQuery(projectId);
        var result = await _mediator.Send(query);

        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager,TeamMember")]
    public async Task<IActionResult> Create([FromBody] CreateIssueCommand command)
    {
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,ProjectManager,TeamMember")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateIssueCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }

        var result = await _mediator.Send(command);

        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Administrator,ProjectManager,TeamMember")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeIssueStatusCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }

        var result = await _mediator.Send(command);

        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("{id}/resolve")]
    [Authorize(Roles = "Administrator,ProjectManager,TeamMember")]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveIssueCommand command)
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
        var command = new DeleteIssueCommand(id);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}
