using VolcanionPM.Domain.Common;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Domain.Entities;

/// <summary>
/// ResourceAllocation Entity - Represents allocation of a user to a project
/// </summary>
public class ResourceAllocation : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public DateRange AllocationPeriod { get; private set; } = null!;
    public ResourceAllocationType Type { get; private set; }
    public decimal AllocationPercentage { get; private set; } // 0-100
    public Money? HourlyRate { get; private set; }
    public string? Notes { get; private set; }

    // Private constructor for EF Core
    private ResourceAllocation() { }

    public static ResourceAllocation Create(
        Guid userId,
        Guid projectId,
        DateRange allocationPeriod,
        ResourceAllocationType type,
        decimal allocationPercentage,
        Money? hourlyRate = null,
        string? notes = null,
        string createdBy = "System")
    {
        if (allocationPercentage < 0 || allocationPercentage > 100)
            throw new ArgumentException("Allocation percentage must be between 0 and 100", nameof(allocationPercentage));

        var allocation = new ResourceAllocation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProjectId = projectId,
            AllocationPeriod = allocationPeriod,
            Type = type,
            AllocationPercentage = allocationPercentage,
            HourlyRate = hourlyRate,
            Notes = notes?.Trim(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        allocation.AddDomainEvent(new ResourceAllocatedEvent(allocation.Id, userId, projectId, allocationPercentage));
        return allocation;
    }

    public void Update(
        DateRange allocationPeriod,
        ResourceAllocationType type,
        decimal allocationPercentage,
        Money? hourlyRate,
        string? notes,
        string updatedBy)
    {
        if (allocationPercentage < 0 || allocationPercentage > 100)
            throw new ArgumentException("Allocation percentage must be between 0 and 100", nameof(allocationPercentage));

        AllocationPeriod = allocationPeriod;
        Type = type;
        AllocationPercentage = allocationPercentage;
        HourlyRate = hourlyRate;
        Notes = notes?.Trim();
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActiveOn(DateTime date)
    {
        return AllocationPeriod.Contains(date);
    }

    public bool IsCurrentlyActive()
    {
        return IsActiveOn(DateTime.UtcNow.Date);
    }
}

public record ResourceAllocatedEvent(
    Guid AllocationId,
    Guid UserId,
    Guid ProjectId,
    decimal AllocationPercentage) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
