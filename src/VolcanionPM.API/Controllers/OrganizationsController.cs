using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionPM.Application.Features.Organizations.Commands.Create;
using VolcanionPM.Application.Features.Organizations.Commands.Delete;
using VolcanionPM.Application.Features.Organizations.Commands.Update;
using VolcanionPM.Application.Features.Organizations.Queries.GetAllOrganizations;
using VolcanionPM.Application.Features.Organizations.Queries.GetOrganizationById;

namespace VolcanionPM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrganizationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrganizationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive)
    {
        var query = new GetAllOrganizationsQuery { IsActive = isActive };
        var result = await _mediator.Send(query);

        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetOrganizationByIdQuery(id);
        var result = await _mediator.Send(query);

        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create([FromBody] CreateOrganizationCommand command)
    {
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrganizationCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }

        var result = await _mediator.Send(command);

        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteOrganizationCommand(id);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}
