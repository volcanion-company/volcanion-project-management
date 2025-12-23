using VolcanionPM.Domain.Common;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Domain.Entities;

/// <summary>
/// Project Aggregate Root - Represents a project in the system
/// </summary>
public class Project : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty; // Unique project identifier (e.g., "PROJ-001")
    public string? Description { get; private set; }
    public ProjectStatus Status { get; private set; }
    public ProjectPriority Priority { get; private set; }
    public DateRange DateRange { get; private set; } = null!;
    public Money Budget { get; private set; } = null!;
    public decimal ProgressPercentage { get; private set; } = 0;

    // Relationships
    public Guid OrganizationId { get; private set; }
    public Organization Organization { get; private set; } = null!;

    public Guid ProjectManagerId { get; private set; }
    public User ProjectManager { get; private set; } = null!;

    // Navigation properties
    private readonly List<ProjectTask> _tasks = new();
    public IReadOnlyCollection<ProjectTask> Tasks => _tasks.AsReadOnly();

    private readonly List<Sprint> _sprints = new();
    public IReadOnlyCollection<Sprint> Sprints => _sprints.AsReadOnly();

    private readonly List<Risk> _risks = new();
    public IReadOnlyCollection<Risk> Risks => _risks.AsReadOnly();

    private readonly List<Issue> _issues = new();
    public IReadOnlyCollection<Issue> Issues => _issues.AsReadOnly();

    private readonly List<Document> _documents = new();
    public IReadOnlyCollection<Document> Documents => _documents.AsReadOnly();

    private readonly List<ResourceAllocation> _resourceAllocations = new();
    public IReadOnlyCollection<ResourceAllocation> ResourceAllocations => _resourceAllocations.AsReadOnly();

    // Private constructor for EF Core
    private Project() { }

    public static Project Create(
        string name,
        string code,
        Guid organizationId,
        Guid projectManagerId,
        DateRange dateRange,
        Money budget,
        ProjectPriority priority = ProjectPriority.Medium,
        string? description = null,
        string createdBy = "System")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Project code is required", nameof(code));

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Code = code.ToUpperInvariant().Trim(),
            Description = description?.Trim(),
            OrganizationId = organizationId,
            ProjectManagerId = projectManagerId,
            DateRange = dateRange,
            Budget = budget,
            Priority = priority,
            Status = ProjectStatus.Planning,
            ProgressPercentage = 0,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        project.AddDomainEvent(new ProjectCreatedEvent(project.Id, project.Name, project.Code));
        return project;
    }

    public void UpdateDetails(
        string name,
        string? description,
        DateRange dateRange,
        Money budget,
        ProjectPriority priority,
        string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        DateRange = dateRange;
        Budget = budget;
        Priority = priority;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectUpdatedEvent(Id, Name));
    }

    public void ChangeStatus(ProjectStatus newStatus, string updatedBy)
    {
        if (Status == newStatus)
            return;

        var oldStatus = Status;
        Status = newStatus;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectStatusChangedEvent(Id, Name, oldStatus, newStatus));
    }

    public void StartProject(string updatedBy)
    {
        if (Status != ProjectStatus.Planning)
            throw new InvalidOperationException("Project can only be started from Planning status");

        ChangeStatus(ProjectStatus.Active, updatedBy);
    }

    public void CompleteProject(string updatedBy)
    {
        if (Status != ProjectStatus.Active)
            throw new InvalidOperationException("Only active projects can be completed");

        ChangeStatus(ProjectStatus.Completed, updatedBy);
        ProgressPercentage = 100;
    }

    public void PutOnHold(string reason, string updatedBy)
    {
        if (Status != ProjectStatus.Active)
            throw new InvalidOperationException("Only active projects can be put on hold");

        ChangeStatus(ProjectStatus.OnHold, updatedBy);
    }

    public void ResumeProject(string updatedBy)
    {
        if (Status != ProjectStatus.OnHold)
            throw new InvalidOperationException("Only on-hold projects can be resumed");

        ChangeStatus(ProjectStatus.Active, updatedBy);
    }

    public void CancelProject(string reason, string updatedBy)
    {
        if (Status == ProjectStatus.Completed || Status == ProjectStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel completed or already cancelled projects");

        ChangeStatus(ProjectStatus.Cancelled, updatedBy);
    }

    public void ChangeProjectManager(Guid newProjectManagerId, string updatedBy)
    {
        if (ProjectManagerId == newProjectManagerId)
            return;

        var oldManagerId = ProjectManagerId;
        ProjectManagerId = newProjectManagerId;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectManagerChangedEvent(Id, oldManagerId, newProjectManagerId));
    }

    public void UpdateProgress(decimal percentage, string updatedBy)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Progress percentage must be between 0 and 100", nameof(percentage));

        ProgressPercentage = percentage;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddTask(ProjectTask task)
    {
        _tasks.Add(task);
    }

    public void AddSprint(Sprint sprint)
    {
        _sprints.Add(sprint);
    }
}

// Domain Events
public record ProjectCreatedEvent(Guid ProjectId, string ProjectName, string ProjectCode) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record ProjectUpdatedEvent(Guid ProjectId, string ProjectName) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record ProjectStatusChangedEvent(
    Guid ProjectId,
    string ProjectName,
    ProjectStatus OldStatus,
    ProjectStatus NewStatus) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record ProjectManagerChangedEvent(
    Guid ProjectId,
    Guid OldProjectManagerId,
    Guid NewProjectManagerId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
