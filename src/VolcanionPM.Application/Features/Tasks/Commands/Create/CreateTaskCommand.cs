using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Tasks;

namespace VolcanionPM.Application.Features.Tasks.Commands.Create;

public record CreateTaskCommand : IRequest<Result<TaskDto>>
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
    public string CreatedBy { get; init; } = string.Empty;
}
