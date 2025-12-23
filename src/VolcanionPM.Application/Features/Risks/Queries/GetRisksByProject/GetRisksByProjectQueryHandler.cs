using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Risks.DTOs;

namespace VolcanionPM.Application.Features.Risks.Queries.GetRisksByProject;

public class GetRisksByProjectQueryHandler : IRequestHandler<GetRisksByProjectQuery, Result<List<RiskDto>>>
{
    private readonly IRiskRepository _riskRepository;

    public GetRisksByProjectQueryHandler(IRiskRepository riskRepository)
    {
        _riskRepository = riskRepository;
    }

    public async Task<Result<List<RiskDto>>> Handle(GetRisksByProjectQuery request, CancellationToken cancellationToken)
    {
        var risks = await _riskRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);

        var dtos = risks.Select(risk => new RiskDto
        {
            Id = risk.Id,
            Title = risk.Title,
            Description = risk.Description,
            Level = risk.Level,
            Status = risk.Status,
            Probability = risk.Probability,
            Impact = risk.Impact,
            RiskScore = risk.GetRiskScore(),
            MitigationStrategy = risk.MitigationStrategy,
            IdentifiedDate = risk.IdentifiedDate,
            ResolvedDate = risk.ResolvedDate,
            ProjectId = risk.ProjectId,
            ProjectName = risk.Project?.Name,
            OwnerId = risk.OwnerId,
            OwnerName = risk.Owner?.GetFullName(),
            CreatedDate = risk.CreatedAt,
            LastModifiedDate = risk.UpdatedAt
        }).ToList();

        return Result<List<RiskDto>>.Success(dtos);
    }
}
