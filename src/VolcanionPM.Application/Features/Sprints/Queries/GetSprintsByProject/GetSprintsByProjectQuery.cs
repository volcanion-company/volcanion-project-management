using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Sprints.DTOs;

namespace VolcanionPM.Application.Features.Sprints.Queries.GetSprintsByProject;

public class GetSprintsByProjectQuery : PagedQuery, IRequest<Result<PagedResult<SprintDto>>>
{
    public Guid ProjectId { get; set; }
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }
    public string SortBy { get; set; } = "sprintnumber";
    public string SortOrder { get; set; } = "desc";
}
