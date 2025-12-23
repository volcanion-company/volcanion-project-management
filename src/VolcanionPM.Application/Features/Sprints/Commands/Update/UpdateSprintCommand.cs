using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Sprints.Commands.Update;

public record UpdateSprintCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string? Goal { get; init; }
    public int? TotalStoryPoints { get; init; }
}
