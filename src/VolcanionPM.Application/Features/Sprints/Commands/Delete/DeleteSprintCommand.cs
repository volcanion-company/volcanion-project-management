using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Sprints.Commands.Delete;

public record DeleteSprintCommand(Guid Id) : IRequest<Result<Unit>>;
