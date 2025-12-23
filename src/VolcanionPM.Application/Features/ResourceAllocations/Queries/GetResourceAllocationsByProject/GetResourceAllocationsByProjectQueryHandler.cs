using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.ResourceAllocations.DTOs;

namespace VolcanionPM.Application.Features.ResourceAllocations.Queries.GetResourceAllocationsByProject;

public class GetResourceAllocationsByProjectQueryHandler : IRequestHandler<GetResourceAllocationsByProjectQuery, Result<List<ResourceAllocationDto>>>
{
    private readonly IResourceAllocationRepository _resourceAllocationRepository;

    public GetResourceAllocationsByProjectQueryHandler(IResourceAllocationRepository resourceAllocationRepository)
    {
        _resourceAllocationRepository = resourceAllocationRepository;
    }

    public async Task<Result<List<ResourceAllocationDto>>> Handle(GetResourceAllocationsByProjectQuery request, CancellationToken cancellationToken)
    {
        var allocations = await _resourceAllocationRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);

        var dtos = allocations.Select(allocation => new ResourceAllocationDto
        {
            Id = allocation.Id,
            UserId = allocation.UserId,
            UserName = allocation.User?.GetFullName(),
            ProjectId = allocation.ProjectId,
            ProjectName = allocation.Project?.Name,
            StartDate = allocation.AllocationPeriod.StartDate,
            EndDate = allocation.AllocationPeriod.EndDate,
            Type = allocation.Type,
            AllocationPercentage = allocation.AllocationPercentage,
            HourlyRateAmount = allocation.HourlyRate?.Amount,
            HourlyRateCurrency = allocation.HourlyRate?.Currency,
            Notes = allocation.Notes,
            IsCurrentlyActive = allocation.IsCurrentlyActive(),
            CreatedDate = allocation.CreatedAt,
            LastModifiedDate = allocation.UpdatedAt
        }).ToList();

        return Result<List<ResourceAllocationDto>>.Success(dtos);
    }
}
