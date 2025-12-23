using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Users.Commands.Delete;

public record DeleteUserCommand(Guid Id) : IRequest<Result<Unit>>;
