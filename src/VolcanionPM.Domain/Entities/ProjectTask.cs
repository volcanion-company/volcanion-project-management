using VolcanionPM.Domain.Common;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Domain.Entities;

/// <summary>
/// ProjectTask Entity - Represents a task or work item in a project
/// </summary>
public class ProjectTask : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty; // e.g., "TASK-001"
    public string? Description { get; private set; }
    public TaskType Type { get; private set; }
    public Enums.TaskStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public int? StoryPoints { get; private set; }
    public decimal EstimatedHours { get; private set; }
    public decimal ActualHours { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // Hierarchy
    public Guid? ParentTaskId { get; private set; }
    public ProjectTask? ParentTask { get; private set; }

    private readonly List<ProjectTask> _subTasks = new();
    public IReadOnlyCollection<ProjectTask> SubTasks => _subTasks.AsReadOnly();

    // Relationships
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public Guid? AssignedToId { get; private set; }
    public User? AssignedTo { get; private set; }

    public Guid? SprintId { get; private set; }
    public Sprint? Sprint { get; private set; }

    // Navigation properties
    private readonly List<TimeEntry> _timeEntries = new();
    public IReadOnlyCollection<TimeEntry> TimeEntries => _timeEntries.AsReadOnly();

    private readonly List<TaskComment> _comments = new();
    public IReadOnlyCollection<TaskComment> Comments => _comments.AsReadOnly();

    // Private constructor for EF Core
    private ProjectTask() { }

    public static ProjectTask Create(
        string title,
        string code,
        Guid projectId,
        TaskType type,
        TaskPriority priority,
        decimal estimatedHours,
        string? description = null,
        Guid? assignedToId = null,
        Guid? sprintId = null,
        Guid? parentTaskId = null,
        DateTime? dueDate = null,
        int? storyPoints = null,
        string createdBy = "System")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title is required", nameof(title));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Task code is required", nameof(code));

        if (estimatedHours < 0)
            throw new ArgumentException("Estimated hours cannot be negative", nameof(estimatedHours));

        var task = new ProjectTask
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Code = code.ToUpperInvariant().Trim(),
            Description = description?.Trim(),
            ProjectId = projectId,
            Type = type,
            Priority = priority,
            Status = Enums.TaskStatus.Backlog,
            EstimatedHours = estimatedHours,
            ActualHours = 0,
            AssignedToId = assignedToId,
            SprintId = sprintId,
            ParentTaskId = parentTaskId,
            DueDate = dueDate,
            StoryPoints = storyPoints,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        task.AddDomainEvent(new TaskCreatedEvent(task.Id, task.Title, task.Code, projectId));
        return task;
    }

    public void UpdateDetails(
        string title,
        string? description,
        TaskType type,
        TaskPriority priority,
        decimal estimatedHours,
        DateTime? dueDate,
        int? storyPoints,
        string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title is required", nameof(title));

        if (estimatedHours < 0)
            throw new ArgumentException("Estimated hours cannot be negative", nameof(estimatedHours));

        Title = title.Trim();
        Description = description?.Trim();
        Type = type;
        Priority = priority;
        EstimatedHours = estimatedHours;
        DueDate = dueDate;
        StoryPoints = storyPoints;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(Enums.TaskStatus newStatus, string updatedBy)
    {
        if (Status == newStatus)
            return;

        var oldStatus = Status;
        Status = newStatus;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;

        if (newStatus == Enums.TaskStatus.Done)
        {
            CompletedAt = DateTime.UtcNow;
        }
        else if (oldStatus == Enums.TaskStatus.Done)
        {
            CompletedAt = null;
        }

        AddDomainEvent(new TaskStatusChangedEvent(Id, Title, oldStatus, newStatus));
    }

    public void AssignTo(Guid userId, string assignedBy)
    {
        var oldAssigneeId = AssignedToId;
        AssignedToId = userId;
        UpdatedBy = assignedBy;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TaskAssignedEvent(Id, Title, oldAssigneeId, userId));
    }

    public void Unassign(string unassignedBy)
    {
        var oldAssigneeId = AssignedToId;
        AssignedToId = null;
        UpdatedBy = unassignedBy;
        UpdatedAt = DateTime.UtcNow;

        if (oldAssigneeId.HasValue)
        {
            AddDomainEvent(new TaskUnassignedEvent(Id, Title, oldAssigneeId.Value));
        }
    }

    public void AddToSprint(Guid sprintId, string updatedBy)
    {
        SprintId = sprintId;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveFromSprint(string updatedBy)
    {
        SprintId = null;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordTime(decimal hours)
    {
        if (hours < 0)
            throw new ArgumentException("Hours cannot be negative", nameof(hours));

        ActualHours += hours;
        UpdatedAt = DateTime.UtcNow;
    }

    public void StartWork(string startedBy)
    {
        if (Status == Enums.TaskStatus.Backlog || Status == Enums.TaskStatus.ToDo)
        {
            ChangeStatus(Enums.TaskStatus.InProgress, startedBy);
        }
    }

    public void MarkAsComplete(string completedBy)
    {
        ChangeStatus(Enums.TaskStatus.Done, completedBy);
    }

    public void Block(string reason, string blockedBy)
    {
        if (Status != Enums.TaskStatus.InProgress)
            throw new InvalidOperationException("Only in-progress tasks can be blocked");

        ChangeStatus(Enums.TaskStatus.Blocked, blockedBy);
        AddDomainEvent(new TaskBlockedEvent(Id, Title, reason));
    }

    public void Unblock(string unblockedBy)
    {
        if (Status != Enums.TaskStatus.Blocked)
            throw new InvalidOperationException("Only blocked tasks can be unblocked");

        ChangeStatus(Enums.TaskStatus.InProgress, unblockedBy);
    }
}

// Domain Events
public record TaskCreatedEvent(Guid TaskId, string Title, string Code, Guid ProjectId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record TaskStatusChangedEvent(
    Guid TaskId,
    string Title,
    Enums.TaskStatus OldStatus,
    Enums.TaskStatus NewStatus) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record TaskAssignedEvent(
    Guid TaskId,
    string Title,
    Guid? OldAssigneeId,
    Guid NewAssigneeId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record TaskUnassignedEvent(Guid TaskId, string Title, Guid OldAssigneeId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record TaskBlockedEvent(Guid TaskId, string Title, string Reason) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
