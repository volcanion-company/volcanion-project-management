using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Issues.DTOs;

namespace VolcanionPM.Application.Features.Issues.Queries.GetIssuesByProject;

public class GetIssuesByProjectQuery : PagedQuery, IRequest<Result<PagedResult<IssueDto>>>
{
    public Guid ProjectId { get; set; }
    public string? Status { get; set; }
    public string? Severity { get; set; }
    public string? SearchTerm { get; set; }
    public string SortBy { get; set; } = "createdat";
    public string SortOrder { get; set; } = "desc";
}
