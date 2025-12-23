using VolcanionPM.Domain.Common;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Domain.Entities;

/// <summary>
/// TimeEntry Entity - Represents time logged on a task
/// </summary>
public class TimeEntry : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid TaskId { get; private set; }
    public ProjectTask Task { get; private set; } = null!;

    public decimal Hours { get; private set; }
    public TimeEntryType Type { get; private set; }
    public DateTime Date { get; private set; }
    public string? Description { get; private set; }
    public bool IsBillable { get; private set; }

    // Private constructor for EF Core
    private TimeEntry() { }

    public static TimeEntry Create(
        Guid userId,
        Guid taskId,
        decimal hours,
        TimeEntryType type,
        DateTime date,
        string? description = null,
        bool isBillable = true,
        string createdBy = "System")
    {
        if (hours <= 0)
            throw new ArgumentException("Hours must be positive", nameof(hours));

        if (hours > 24)
            throw new ArgumentException("Cannot log more than 24 hours per entry", nameof(hours));

        var timeEntry = new TimeEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TaskId = taskId,
            Hours = hours,
            Type = type,
            Date = date.Date,
            Description = description?.Trim(),
            IsBillable = isBillable,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        timeEntry.AddDomainEvent(new TimeEntryLoggedEvent(timeEntry.Id, userId, taskId, hours));
        return timeEntry;
    }

    public void Update(decimal hours, TimeEntryType type, DateTime date, string? description, bool isBillable, string updatedBy)
    {
        if (hours <= 0)
            throw new ArgumentException("Hours must be positive", nameof(hours));

        if (hours > 24)
            throw new ArgumentException("Cannot log more than 24 hours per entry", nameof(hours));

        Hours = hours;
        Type = type;
        Date = date.Date;
        Description = description?.Trim();
        IsBillable = isBillable;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}

public record TimeEntryLoggedEvent(Guid TimeEntryId, Guid UserId, Guid TaskId, decimal Hours) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
