namespace VolcanionPM.Domain.Common;

/// <summary>
/// Base interface for domain events
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
