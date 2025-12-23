using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.TimeEntries.Commands.Create;

public record CreateTimeEntryCommand : IRequest<Result<Guid>>
{
    public Guid UserId { get; init; }
    public Guid TaskId { get; init; }
    public decimal Hours { get; init; }
    public TimeEntryType Type { get; init; }
    public DateTime Date { get; init; }
    public string? Description { get; init; }
    public bool IsBillable { get; init; } = true;
}
