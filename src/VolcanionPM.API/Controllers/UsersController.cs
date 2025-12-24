using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionPM.Application.Common.Models;
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

    /// <summary>
    /// Get all users with pagination, filtering, and sorting
    /// </summary>
    /// <param name="organizationId">Filter by organization ID</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="role">Filter by role (Administrator, ProjectManager, TeamMember)</param>
    /// <param name="searchTerm">Search in first name, last name, or email</param>
    /// <param name="sortBy">Sort field: firstname, lastname, email, role, isactive, createdat</param>
    /// <param name="sortOrder">Sort order: asc or desc</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? organizationId,
        [FromQuery] bool? isActive,
        [FromQuery] string? role,
        [FromQuery] string? searchTerm,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllUsersQuery
        {
            OrganizationId = organizationId,
            IsActive = isActive,
            Role = role,
            SearchTerm = searchTerm,
            SortBy = sortBy ?? "createdat",
            SortOrder = sortOrder ?? "desc",
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);

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
