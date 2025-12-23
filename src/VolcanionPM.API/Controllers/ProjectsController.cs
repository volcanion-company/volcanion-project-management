using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionPM.Application.Features.Projects.Commands.Create;
using VolcanionPM.Application.Features.Projects.Commands.Update;
using VolcanionPM.Application.Features.Projects.Commands.Delete;
using VolcanionPM.Application.Features.Projects.Queries.GetAll;
using VolcanionPM.Application.Features.Projects.Queries.GetById;
using VolcanionPM.Application.DTOs.Projects;

namespace VolcanionPM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(IMediator mediator, ILogger<ProjectsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all projects
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? organizationId, CancellationToken cancellationToken)
    {
        var query = new GetAllProjectsQuery(organizationId);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error, errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get project by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProjectByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto, CancellationToken cancellationToken)
    {
        var command = new CreateProjectCommand
        {
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            Priority = dto.Priority,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            BudgetAmount = dto.BudgetAmount,
            BudgetCurrency = dto.BudgetCurrency,
            OrganizationId = dto.OrganizationId,
            CreatedBy = User.Identity?.Name ?? "Anonymous"
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error, errors = result.Errors });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update an existing project
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { error = "ID mismatch" });
        }

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete a project
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteProjectCommand { Id = id, DeletedBy = User.Identity?.Name ?? "Anonymous" };
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }

        return NoContent();
    }
}
