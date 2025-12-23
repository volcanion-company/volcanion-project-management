using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Sprints.Commands.StartSprint;

public record StartSprintCommand(Guid Id) : IRequest<Result<Unit>>;
