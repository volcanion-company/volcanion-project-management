using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Users.DTOs;

namespace VolcanionPM.Application.Features.Users.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<Result<List<UserDto>>>
{
    public Guid? OrganizationId { get; init; }
    public bool? IsActive { get; init; }
}
