using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Projects.Commands.Create;
using VolcanionPM.Application.Features.Projects.Commands.Update;
using VolcanionPM.Application.Features.Projects.Commands.Delete;
using VolcanionPM.Application.Features.Projects.Queries.GetAll;
using VolcanionPM.Application.Features.Projects.Queries.GetById;
using VolcanionPM.Application.DTOs.Projects;
using VolcanionPM.Application.Common.Interfaces;

namespace VolcanionPM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProjectsController> _logger;
    private readonly IAuthorizationService _authorizationService;
    private readonly IProjectRepository _projectRepository;

    public ProjectsController(
        IMediator mediator, 
        ILogger<ProjectsController> logger,
        IAuthorizationService authorizationService,
        IProjectRepository projectRepository)
    {
        _mediator = mediator;
        _logger = logger;
        _authorizationService = authorizationService;
        _projectRepository = projectRepository;
    }

    /// <summary>
    /// Get all projects with pagination, filtering, and sorting
    /// </summary>
    /// <param name="organizationId">Filter by organization</param>
    /// <param name="status">Filter by status (Planning, Active, OnHold, Completed, Cancelled, Archived)</param>
    /// <param name="priority">Filter by priority (Low, Medium, High, Critical)</param>
    /// <param name="searchTerm">Search in name, code, or description</param>
    /// <param name="sortBy">Sort field (name, code, status, priority, startdate, enddate, createdat)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? organizationId,
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] string? searchTerm,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllProjectsQuery
        {
            OrganizationId = organizationId,
            Status = status,
            Priority = priority,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortOrder = sortOrder,
            Page = page,
            PageSize = pageSize
        };

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
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { error = "ID mismatch" });
        }

        // Get project for authorization check
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project == null)
        {
            return NotFound(new { error = "Project not found" });
        }

        // Resource-based authorization check
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, project, "CanEditProject");
        if (!authorizationResult.Succeeded)
        {
            _logger.LogWarning("User {UserId} denied permission to edit project {ProjectId}", User.Identity?.Name, id);
            return Forbid();
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
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        // Get project for authorization check
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project == null)
        {
            return NotFound(new { error = "Project not found" });
        }

        // Resource-based authorization check (Admin only)
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, project, "CanDeleteProject");
        if (!authorizationResult.Succeeded)
        {
            _logger.LogWarning("User {UserId} denied permission to delete project {ProjectId}", User.Identity?.Name, id);
            return Forbid();
        }

        var command = new DeleteProjectCommand { Id = id, DeletedBy = User.Identity?.Name ?? "Anonymous" };
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }

        return NoContent();
    }
}
