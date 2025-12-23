using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Sprints.DTOs;

namespace VolcanionPM.Application.Features.Sprints.Queries.GetSprintById;

public record GetSprintByIdQuery(Guid Id) : IRequest<Result<SprintDto>>;
