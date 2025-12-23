using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Users.Commands.Create;

public record CreateUserCommand : IRequest<Result<Guid>>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public string? PhoneNumber { get; init; }
    public Guid OrganizationId { get; init; }
}
