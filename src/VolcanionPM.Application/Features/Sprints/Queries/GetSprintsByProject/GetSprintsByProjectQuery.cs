using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Sprints.DTOs;

namespace VolcanionPM.Application.Features.Sprints.Queries.GetSprintsByProject;

public record GetSprintsByProjectQuery(Guid ProjectId) : IRequest<Result<List<SprintDto>>>;
