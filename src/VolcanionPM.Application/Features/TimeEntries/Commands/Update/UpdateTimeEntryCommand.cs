using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.TimeEntries.Commands.Update;

public record UpdateTimeEntryCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public decimal Hours { get; init; }
    public TimeEntryType Type { get; init; }
    public DateTime Date { get; init; }
    public string? Description { get; init; }
    public bool IsBillable { get; init; }
}
