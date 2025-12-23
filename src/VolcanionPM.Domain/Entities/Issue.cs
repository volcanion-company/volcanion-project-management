using VolcanionPM.Domain.Common;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Domain.Entities;

/// <summary>
/// Issue Entity - Represents an issue in a project
/// </summary>
public class Issue : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public IssueStatus Status { get; private set; }
    public IssueSeverity Severity { get; private set; }
    public DateTime? ResolvedDate { get; private set; }
    public string? Resolution { get; private set; }

    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public Guid? ReportedById { get; private set; }
    public User? ReportedBy { get; private set; }

    public Guid? AssignedToId { get; private set; }
    public User? AssignedTo { get; private set; }

    // Private constructor for EF Core
    private Issue() { }

    public static Issue Create(
        string title,
        string description,
        Guid projectId,
        IssueSeverity severity,
        Guid? reportedById = null,
        Guid? assignedToId = null,
        string createdBy = "System")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Issue title is required", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Issue description is required", nameof(description));

        var issue = new Issue
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description.Trim(),
            ProjectId = projectId,
            Severity = severity,
            Status = IssueStatus.Open,
            ReportedById = reportedById,
            AssignedToId = assignedToId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        issue.AddDomainEvent(new IssueCreatedEvent(issue.Id, title, projectId, severity));
        return issue;
    }

    public void Update(string title, string description, IssueSeverity severity, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Issue title is required", nameof(title));

        Title = title.Trim();
        Description = description.Trim();
        Severity = severity;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(IssueStatus newStatus, string updatedBy)
    {
        if (Status == newStatus)
            return;

        Status = newStatus;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;

        if (newStatus == IssueStatus.Resolved || newStatus == IssueStatus.Closed)
        {
            ResolvedDate = DateTime.UtcNow;
        }
    }

    public void Resolve(string resolution, string resolvedBy)
    {
        Resolution = resolution;
        ChangeStatus(IssueStatus.Resolved, resolvedBy);
        AddDomainEvent(new IssueResolvedEvent(Id, Title, ProjectId));
    }

    public void Close(string closedBy)
    {
        ChangeStatus(IssueStatus.Closed, closedBy);
    }

    public void Reopen(string reopenedBy)
    {
        ChangeStatus(IssueStatus.Reopened, reopenedBy);
        ResolvedDate = null;
    }

    public void AssignTo(Guid userId, string assignedBy)
    {
        AssignedToId = userId;
        UpdatedBy = assignedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}

public record IssueCreatedEvent(Guid IssueId, string Title, Guid ProjectId, IssueSeverity Severity) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record IssueResolvedEvent(Guid IssueId, string Title, Guid ProjectId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
