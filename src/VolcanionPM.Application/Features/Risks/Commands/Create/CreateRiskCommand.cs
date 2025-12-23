using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Risks.Commands.Create;

public record CreateRiskCommand : IRequest<Result<Guid>>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public RiskLevel Level { get; init; }
    public Guid ProjectId { get; init; }
    public decimal Probability { get; init; }
    public decimal Impact { get; init; }
    public Guid? OwnerId { get; init; }
    public string? MitigationStrategy { get; init; }
}
