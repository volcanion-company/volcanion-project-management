using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.TimeEntries.DTOs;

namespace VolcanionPM.Application.Features.TimeEntries.Queries.GetTimeEntriesByUser;

public class GetTimeEntriesByUserQuery : PagedQuery, IRequest<Result<PagedResult<TimeEntryDto>>>
{
    public Guid UserId { get; set; }
    public Guid? TaskId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Type { get; set; }
    public bool? IsBillable { get; set; }
    public string SortBy { get; set; } = "date";
    public string SortOrder { get; set; } = "desc";
}
