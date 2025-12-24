using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Risks.DTOs;

namespace VolcanionPM.Application.Features.Risks.Queries.GetRisksByProject;

public class GetRisksByProjectQuery : PagedQuery, IRequest<Result<PagedResult<RiskDto>>>
{
    public Guid ProjectId { get; set; }
    public string? Level { get; set; }
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }
    public string SortBy { get; set; } = "riskscore";
    public string SortOrder { get; set; } = "desc";
}
