using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Organizations.DTOs;

namespace VolcanionPM.Application.Features.Organizations.Queries.GetAllOrganizations;

public record GetAllOrganizationsQuery : IRequest<Result<List<OrganizationDto>>>
{
    public bool? IsActive { get; init; }
}
