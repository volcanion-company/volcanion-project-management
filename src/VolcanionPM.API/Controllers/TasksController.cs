using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionPM.Application.Features.Tasks.Commands.Create;
using VolcanionPM.Application.Features.Tasks.Commands.Update;
using VolcanionPM.Application.Features.Tasks.Queries.GetById;
using VolcanionPM.Application.Features.Tasks.Queries.GetTasksByProject;
using VolcanionPM.Application.DTOs.Tasks;

namespace VolcanionPM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ILogger<TasksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get task by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetTaskByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get tasks by project ID
    /// </summary>
    [HttpGet("project/{projectId:guid}")]
    [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProject(Guid projectId, CancellationToken cancellationToken)
    {
        var query = new GetTasksByProjectQuery { ProjectId = projectId };
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto, CancellationToken cancellationToken)
    {
        var command = new CreateTaskCommand
        {
            Title = dto.Title,
            Description = dto.Description,
            Type = dto.Type,
            Priority = dto.Priority,
            StoryPoints = dto.StoryPoints,
            EstimatedHours = dto.EstimatedHours,
            DueDate = dto.DueDate,
            AssignedToId = dto.AssignedToId,
            ProjectId = dto.ProjectId,
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
    /// Update an existing task
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskCommand command, CancellationToken cancellationToken)
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
}
