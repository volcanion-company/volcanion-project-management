using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Projects;

namespace VolcanionPM.Application.Features.Projects.Queries.GetAll;

public class GetAllProjectsQuery : PagedQuery, IRequest<Result<PagedResult<ProjectDto>>>
{
    public Guid? OrganizationId { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}
