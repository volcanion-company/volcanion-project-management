using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Risks.DTOs;

namespace VolcanionPM.Application.Features.Risks.Queries.GetRisksByProject;

public class GetRisksByProjectQueryHandler : IRequestHandler<GetRisksByProjectQuery, Result<PagedResult<RiskDto>>>
{
    private readonly IRiskRepository _riskRepository;

    public GetRisksByProjectQueryHandler(IRiskRepository riskRepository)
    {
        _riskRepository = riskRepository;
    }

    public async Task<Result<PagedResult<RiskDto>>> Handle(GetRisksByProjectQuery request, CancellationToken cancellationToken)
    {
        var query = _riskRepository.GetQueryable()
            .Where(r => r.ProjectId == request.ProjectId);

        // Filter by Level
        if (!string.IsNullOrWhiteSpace(request.Level))
        {
            query = query.Where(r => r.Level.ToString().ToLower() == request.Level.ToLower());
        }

        // Filter by Status
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(r => r.Status.ToString().ToLower() == request.Status.ToLower());
        }

        // Search in Title, Description
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(r =>
                r.Title.ToLower().Contains(searchLower) ||
                (r.Description != null && r.Description.ToLower().Contains(searchLower)));
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        // Get total count
        var totalCount = query.Count();

        // Apply pagination
        var risks = query.Skip(request.Skip).Take(request.Take).ToList();

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

        var pagedResult = PagedResult<RiskDto>.Create(dtos, request.Page, request.PageSize, totalCount);
        return Result<PagedResult<RiskDto>>.Success(pagedResult);
    }

    private IQueryable<Domain.Entities.Risk> ApplySorting(
        IQueryable<Domain.Entities.Risk> query,
        string sortBy,
        string sortOrder)
    {
        var isDescending = sortOrder.ToLower() == "desc";

        query = sortBy.ToLower() switch
        {
            "title" => isDescending ? query.OrderByDescending(r => r.Title) : query.OrderBy(r => r.Title),
            "level" => isDescending ? query.OrderByDescending(r => r.Level) : query.OrderBy(r => r.Level),
            "status" => isDescending ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
            "probability" => isDescending ? query.OrderByDescending(r => r.Probability) : query.OrderBy(r => r.Probability),
            "impact" => isDescending ? query.OrderByDescending(r => r.Impact) : query.OrderBy(r => r.Impact),
            "riskscore" => isDescending ? query.OrderByDescending(r => r.Probability * r.Impact) : query.OrderBy(r => r.Probability * r.Impact),
            "createdat" => isDescending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.Probability * r.Impact)
        };

        return query;
    }
}
