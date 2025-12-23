using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Risks.DTOs;

namespace VolcanionPM.Application.Features.Risks.Queries.GetRiskById;

public record GetRiskByIdQuery(Guid Id) : IRequest<Result<RiskDto>>;
