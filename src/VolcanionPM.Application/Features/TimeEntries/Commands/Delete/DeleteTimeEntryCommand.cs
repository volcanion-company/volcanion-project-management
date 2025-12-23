using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.TimeEntries.Commands.Delete;

public record DeleteTimeEntryCommand(Guid Id) : IRequest<Result<Unit>>;
