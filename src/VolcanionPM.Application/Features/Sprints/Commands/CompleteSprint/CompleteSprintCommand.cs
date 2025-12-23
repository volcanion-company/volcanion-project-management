using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Sprints.Commands.CompleteSprint;

public record CompleteSprintCommand(Guid Id) : IRequest<Result<Unit>>;
