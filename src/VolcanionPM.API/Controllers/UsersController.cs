using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionPM.Application.Features.Users.Commands.Create;
using VolcanionPM.Application.Features.Users.Commands.Delete;
using VolcanionPM.Application.Features.Users.Commands.Update;
using VolcanionPM.Application.Features.Users.Queries.GetAllUsers;
using VolcanionPM.Application.Features.Users.Queries.GetUserById;

namespace VolcanionPM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? organizationId, [FromQuery] bool? isActive)
    {
        var query = new GetAllUsersQuery
        {
            OrganizationId = organizationId,
            IsActive = isActive
        };

        var result = await _mediator.Send(query);

        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query);

        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserCommand command)
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
        var command = new DeleteUserCommand(id);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}
