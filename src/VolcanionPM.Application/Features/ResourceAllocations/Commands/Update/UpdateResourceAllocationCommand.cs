using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.ResourceAllocations.Commands.Update;

public record UpdateResourceAllocationCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public ResourceAllocationType Type { get; init; }
    public decimal AllocationPercentage { get; init; }
    public decimal? HourlyRateAmount { get; init; }
    public string? HourlyRateCurrency { get; init; }
    public string? Notes { get; init; }
}
