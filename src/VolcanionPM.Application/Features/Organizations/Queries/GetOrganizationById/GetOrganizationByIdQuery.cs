using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Organizations.DTOs;

namespace VolcanionPM.Application.Features.Organizations.Queries.GetOrganizationById;

public record GetOrganizationByIdQuery(Guid Id) : IRequest<Result<OrganizationDto>>;
