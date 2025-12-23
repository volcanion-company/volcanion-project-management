using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Auth;

namespace VolcanionPM.Application.Features.Auth.Commands.Register;

public record RegisterCommand : IRequest<Result<AuthResponseDto>>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public Guid OrganizationId { get; init; }
}
