using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.ResourceAllocations.DTOs;

namespace VolcanionPM.Application.Features.ResourceAllocations.Queries.GetResourceAllocationById;

public class GetResourceAllocationByIdQueryHandler : IRequestHandler<GetResourceAllocationByIdQuery, Result<ResourceAllocationDto>>
{
    private readonly IResourceAllocationRepository _resourceAllocationRepository;

    public GetResourceAllocationByIdQueryHandler(IResourceAllocationRepository resourceAllocationRepository)
    {
        _resourceAllocationRepository = resourceAllocationRepository;
    }

    public async Task<Result<ResourceAllocationDto>> Handle(GetResourceAllocationByIdQuery request, CancellationToken cancellationToken)
    {
        var allocation = await _resourceAllocationRepository.GetByIdAsync(request.Id, cancellationToken);

        if (allocation == null)
        {
            return Result<ResourceAllocationDto>.Failure("Resource allocation not found");
        }

        var dto = new ResourceAllocationDto
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
        };

        return Result<ResourceAllocationDto>.Success(dto);
    }
}
