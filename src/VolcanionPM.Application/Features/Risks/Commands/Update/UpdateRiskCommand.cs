using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Risks.Commands.Update;

public record UpdateRiskCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public RiskLevel Level { get; init; }
    public decimal Probability { get; init; }
    public decimal Impact { get; init; }
    public string? MitigationStrategy { get; init; }
}
