using VolcanionPM.Domain.Common;

namespace VolcanionPM.Domain.Entities;

/// <summary>
/// TaskComment Entity - Represents a comment on a task
/// </summary>
public class TaskComment : BaseEntity
{
    public string Content { get; private set; } = string.Empty;
    public bool IsEdited { get; private set; }

    public Guid TaskId { get; private set; }
    public ProjectTask Task { get; private set; } = null!;

    public Guid AuthorId { get; private set; }
    public User Author { get; private set; } = null!;

    // Private constructor for EF Core
    private TaskComment() { }

    public static TaskComment Create(
        Guid taskId,
        Guid authorId,
        string content,
        string createdBy = "System")
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Comment content is required", nameof(content));

        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            AuthorId = authorId,
            Content = content.Trim(),
            IsEdited = false,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        comment.AddDomainEvent(new TaskCommentAddedEvent(comment.Id, taskId, authorId));
        return comment;
    }

    public void Update(string content, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Comment content is required", nameof(content));

        Content = content.Trim();
        IsEdited = true;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}

public record TaskCommentAddedEvent(Guid CommentId, Guid TaskId, Guid AuthorId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
