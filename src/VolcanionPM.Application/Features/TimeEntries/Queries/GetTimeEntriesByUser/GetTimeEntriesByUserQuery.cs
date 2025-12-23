using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.TimeEntries.DTOs;

namespace VolcanionPM.Application.Features.TimeEntries.Queries.GetTimeEntriesByUser;

public record GetTimeEntriesByUserQuery : IRequest<Result<List<TimeEntryDto>>>
{
    public Guid UserId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
