using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Users.Commands.Update;

public record UpdateUserCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public string? PhoneNumber { get; init; }
    public bool IsActive { get; init; }
}
