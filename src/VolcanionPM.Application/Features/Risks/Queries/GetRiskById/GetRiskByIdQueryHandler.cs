using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Risks.DTOs;

namespace VolcanionPM.Application.Features.Risks.Queries.GetRiskById;

public class GetRiskByIdQueryHandler : IRequestHandler<GetRiskByIdQuery, Result<RiskDto>>
{
    private readonly IRiskRepository _riskRepository;

    public GetRiskByIdQueryHandler(IRiskRepository riskRepository)
    {
        _riskRepository = riskRepository;
    }

    public async Task<Result<RiskDto>> Handle(GetRiskByIdQuery request, CancellationToken cancellationToken)
    {
        var risk = await _riskRepository.GetByIdAsync(request.Id, cancellationToken);

        if (risk == null)
        {
            return Result<RiskDto>.Failure("Risk not found");
        }

        var dto = new RiskDto
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
        };

        return Result<RiskDto>.Success(dto);
    }
}
