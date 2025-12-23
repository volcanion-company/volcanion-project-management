namespace VolcanionPM.Domain.Common;

/// <summary>
/// Base class for aggregate roots
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
    // Aggregate roots are the only entities that can be retrieved directly from repositories
    // They maintain consistency boundaries
}
