using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Organizations.DTOs;

namespace VolcanionPM.Application.Features.Organizations.Queries.GetAllOrganizations;

public class GetAllOrganizationsQuery : PagedQuery, IRequest<Result<PagedResult<OrganizationDto>>>
{
    public bool? IsActive { get; set; }
    public string? SearchTerm { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortOrder { get; set; } = "asc";
}
