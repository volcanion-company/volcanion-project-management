using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Projects;

namespace VolcanionPM.Application.Features.Projects.Queries.GetAll;

public record GetAllProjectsQuery(Guid? OrganizationId = null) : IRequest<Result<List<ProjectDto>>>;
