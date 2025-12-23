using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionPM.Application.Features.Auth.Commands.Login;
using VolcanionPM.Application.Features.Auth.Commands.Register;
using VolcanionPM.Application.DTOs.Auth;

namespace VolcanionPM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(dto.Email, dto.Password);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
    {
        var command = new RegisterCommand
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Password = dto.Password,
            OrganizationId = dto.OrganizationId
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error, errors = result.Errors });
        }

        return CreatedAtAction(nameof(Login), null, result.Data);
    }

    /// <summary>
    /// Get current user info
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var firstName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value;
        var lastName = User.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var userDto = new UserDto
        {
            Id = Guid.Parse(userId),
            FirstName = firstName ?? string.Empty,
            LastName = lastName ?? string.Empty,
            Email = email ?? string.Empty,
            Role = role ?? string.Empty,
            IsActive = true
        };

        return Ok(userDto);
    }
}
