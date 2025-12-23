using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.ResourceAllocations.DTOs;

namespace VolcanionPM.Application.Features.ResourceAllocations.Queries.GetResourceAllocationById;

public record GetResourceAllocationByIdQuery(Guid Id) : IRequest<Result<ResourceAllocationDto>>;
