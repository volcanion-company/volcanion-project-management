using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.TimeEntries.DTOs;

public class TimeEntryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public Guid TaskId { get; set; }
    public string? TaskTitle { get; set; }
    public decimal Hours { get; set; }
    public TimeEntryType Type { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public bool IsBillable { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
