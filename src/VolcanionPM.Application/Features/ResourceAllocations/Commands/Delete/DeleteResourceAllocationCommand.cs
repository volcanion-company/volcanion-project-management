using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.ResourceAllocations.Commands.Delete;

public record DeleteResourceAllocationCommand(Guid Id) : IRequest<Result<Unit>>;
