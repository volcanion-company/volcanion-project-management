using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Sprints.DTOs;

public class SprintDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public int SprintNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SprintStatus Status { get; set; }
    public int? TotalStoryPoints { get; set; }
    public int? CompletedStoryPoints { get; set; }
    public Guid ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
