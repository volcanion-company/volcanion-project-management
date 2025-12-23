using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Tasks;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Tasks.Commands.Update;

public record UpdateTaskCommand : IRequest<Result<TaskDto>>
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskType Type { get; init; }
    public TaskPriority Priority { get; init; }
    public decimal EstimatedHours { get; init; }
    public int? StoryPoints { get; init; }
    public DateTime? DueDate { get; init; }
    public Guid? AssignedToId { get; init; }
    public string UpdatedBy { get; init; } = "System";
}
