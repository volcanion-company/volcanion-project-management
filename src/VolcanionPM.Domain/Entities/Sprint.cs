using VolcanionPM.Domain.Common;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Domain.Entities;

/// <summary>
/// Sprint Entity - Represents a sprint/iteration in agile project management
/// </summary>
public class Sprint : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Goal { get; private set; }
    public DateRange DateRange { get; private set; } = null!;
    public SprintStatus Status { get; private set; }
    public int SprintNumber { get; private set; }
    public int? TotalStoryPoints { get; private set; }
    public int? CompletedStoryPoints { get; private set; }

    // Relationships
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    // Navigation properties
    private readonly List<ProjectTask> _tasks = new();
    public IReadOnlyCollection<ProjectTask> Tasks => _tasks.AsReadOnly();

    // Private constructor for EF Core
    private Sprint() { }

    public static Sprint Create(
        string name,
        int sprintNumber,
        Guid projectId,
        DateRange dateRange,
        string? goal = null,
        int? totalStoryPoints = null,
        string createdBy = "System")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Sprint name is required", nameof(name));

        if (sprintNumber <= 0)
            throw new ArgumentException("Sprint number must be positive", nameof(sprintNumber));

        var sprint = new Sprint
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            SprintNumber = sprintNumber,
            ProjectId = projectId,
            DateRange = dateRange,
            Goal = goal?.Trim(),
            Status = SprintStatus.Planned,
            TotalStoryPoints = totalStoryPoints,
            CompletedStoryPoints = 0,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        sprint.AddDomainEvent(new SprintCreatedEvent(sprint.Id, sprint.Name, projectId));
        return sprint;
    }

    public void UpdateDetails(
        string name,
        string? goal,
        DateRange dateRange,
        int? totalStoryPoints,
        string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Sprint name is required", nameof(name));

        if (Status == SprintStatus.Completed)
            throw new InvalidOperationException("Cannot update completed sprint");

        Name = name.Trim();
        Goal = goal?.Trim();
        DateRange = dateRange;
        TotalStoryPoints = totalStoryPoints;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Start(string startedBy)
    {
        if (Status != SprintStatus.Planned)
            throw new InvalidOperationException("Only planned sprints can be started");

        if (DateTime.UtcNow < DateRange.StartDate)
            throw new InvalidOperationException("Cannot start sprint before start date");

        Status = SprintStatus.Active;
        UpdatedBy = startedBy;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SprintStartedEvent(Id, Name, ProjectId));
    }

    public void Complete(string completedBy)
    {
        if (Status != SprintStatus.Active)
            throw new InvalidOperationException("Only active sprints can be completed");

        Status = SprintStatus.Completed;
        UpdatedBy = completedBy;
        UpdatedAt = DateTime.UtcNow;

        // Calculate completed story points
        CompletedStoryPoints = _tasks
            .Where(t => t.Status == Enums.TaskStatus.Done && t.StoryPoints.HasValue)
            .Sum(t => t.StoryPoints!.Value);

        AddDomainEvent(new SprintCompletedEvent(Id, Name, ProjectId, CompletedStoryPoints ?? 0, TotalStoryPoints ?? 0));
    }

    public void Cancel(string reason, string cancelledBy)
    {
        if (Status == SprintStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed sprint");

        Status = SprintStatus.Cancelled;
        UpdatedBy = cancelledBy;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SprintCancelledEvent(Id, Name, ProjectId, reason));
    }

    public void AddTask(ProjectTask task)
    {
        if (Status == SprintStatus.Completed)
            throw new InvalidOperationException("Cannot add tasks to completed sprint");

        _tasks.Add(task);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveTask(ProjectTask task)
    {
        if (Status == SprintStatus.Completed)
            throw new InvalidOperationException("Cannot remove tasks from completed sprint");

        _tasks.Remove(task);
        UpdatedAt = DateTime.UtcNow;
    }

    public int GetCompletedTaskCount()
    {
        return _tasks.Count(t => t.Status == Enums.TaskStatus.Done);
    }

    public int GetTotalTaskCount()
    {
        return _tasks.Count;
    }

    public decimal GetCompletionPercentage()
    {
        var totalTasks = GetTotalTaskCount();
        if (totalTasks == 0)
            return 0;

        return (decimal)GetCompletedTaskCount() / totalTasks * 100;
    }
}

// Domain Events
public record SprintCreatedEvent(Guid SprintId, string SprintName, Guid ProjectId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record SprintStartedEvent(Guid SprintId, string SprintName, Guid ProjectId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record SprintCompletedEvent(
    Guid SprintId,
    string SprintName,
    Guid ProjectId,
    int CompletedStoryPoints,
    int TotalStoryPoints) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record SprintCancelledEvent(Guid SprintId, string SprintName, Guid ProjectId, string Reason) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
