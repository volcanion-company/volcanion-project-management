using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionPM.Application.Features.ResourceAllocations.Commands.Create;
using VolcanionPM.Application.Features.ResourceAllocations.Commands.Delete;
using VolcanionPM.Application.Features.ResourceAllocations.Commands.Update;
using VolcanionPM.Application.Features.ResourceAllocations.Queries.GetResourceAllocationById;
using VolcanionPM.Application.Features.ResourceAllocations.Queries.GetResourceAllocationsByProject;

namespace VolcanionPM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ResourceAllocationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ResourceAllocationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetResourceAllocationByIdQuery(id);
        var result = await _mediator.Send(query);

        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetByProject(Guid projectId)
    {
        var query = new GetResourceAllocationsByProjectQuery(projectId);
        var result = await _mediator.Send(query);

        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> Create([FromBody] CreateResourceAllocationCommand command)
    {
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateResourceAllocationCommand command)
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
        var command = new DeleteResourceAllocationCommand(id);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}
