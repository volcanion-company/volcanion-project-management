using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Sprints.Commands.Create;

public record CreateSprintCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = string.Empty;
    public int SprintNumber { get; init; }
    public Guid ProjectId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string? Goal { get; init; }
    public int? TotalStoryPoints { get; init; }
}
