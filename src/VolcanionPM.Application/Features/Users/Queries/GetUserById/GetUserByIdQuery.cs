using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Users.DTOs;

namespace VolcanionPM.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDto>>;
