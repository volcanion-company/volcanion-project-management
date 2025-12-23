using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.ResourceAllocations.DTOs;

namespace VolcanionPM.Application.Features.ResourceAllocations.Queries.GetResourceAllocationsByProject;

public record GetResourceAllocationsByProjectQuery(Guid ProjectId) : IRequest<Result<List<ResourceAllocationDto>>>;
