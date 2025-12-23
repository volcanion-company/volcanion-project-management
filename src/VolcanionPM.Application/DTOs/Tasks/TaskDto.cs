namespace VolcanionPM.Application.DTOs.Tasks;

public record TaskDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public int? StoryPoints { get; init; }
    public decimal EstimatedHours { get; init; }
    public decimal ActualHours { get; init; }
    public DateTime? DueDate { get; init; }
    public Guid? AssignedToId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid? SprintId { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateTaskDto
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public int? StoryPoints { get; init; }
    public decimal EstimatedHours { get; init; }
    public DateTime? DueDate { get; init; }
    public Guid? AssignedToId { get; init; }
    public Guid ProjectId { get; init; }
}

public record UpdateTaskDto
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Priority { get; init; } = string.Empty;
    public int? StoryPoints { get; init; }
    public decimal EstimatedHours { get; init; }
    public DateTime? DueDate { get; init; }
}
