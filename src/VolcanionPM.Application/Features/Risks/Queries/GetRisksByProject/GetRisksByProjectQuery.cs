using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Risks.DTOs;

namespace VolcanionPM.Application.Features.Risks.Queries.GetRisksByProject;

public record GetRisksByProjectQuery(Guid ProjectId) : IRequest<Result<List<RiskDto>>>;
