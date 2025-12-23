using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Risks.Commands.Delete;

public record DeleteRiskCommand(Guid Id) : IRequest<Result<Unit>>;
