using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.ResourceAllocations.Commands.Create;

public record CreateResourceAllocationCommand : IRequest<Result<Guid>>
{
    public Guid UserId { get; init; }
    public Guid ProjectId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public ResourceAllocationType Type { get; init; }
    public decimal AllocationPercentage { get; init; }
    public decimal? HourlyRateAmount { get; init; }
    public string? HourlyRateCurrency { get; init; }
    public string? Notes { get; init; }
}
