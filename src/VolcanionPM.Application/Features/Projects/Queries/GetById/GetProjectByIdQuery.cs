using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Projects;

namespace VolcanionPM.Application.Features.Projects.Queries.GetById;

public record GetProjectByIdQuery(Guid Id) : IRequest<Result<ProjectDto>>;
