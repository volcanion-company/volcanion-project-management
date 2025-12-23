using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.ResourceAllocations.DTOs;

public class ResourceAllocationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public Guid ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ResourceAllocationType Type { get; set; }
    public decimal AllocationPercentage { get; set; }
    public decimal? HourlyRateAmount { get; set; }
    public string? HourlyRateCurrency { get; set; }
    public string? Notes { get; set; }
    public bool IsCurrentlyActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
