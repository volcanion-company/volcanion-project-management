using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Issues.DTOs;

public class IssueDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IssueStatus Status { get; set; }
    public IssueSeverity Severity { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public string? Resolution { get; set; }
    public Guid ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public Guid? ReportedById { get; set; }
    public string? ReportedByName { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
