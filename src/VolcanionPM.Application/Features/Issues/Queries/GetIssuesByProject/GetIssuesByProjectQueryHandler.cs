using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Issues.DTOs;

namespace VolcanionPM.Application.Features.Issues.Queries.GetIssuesByProject;

public class GetIssuesByProjectQueryHandler : IRequestHandler<GetIssuesByProjectQuery, Result<PagedResult<IssueDto>>>
{
    private readonly IIssueRepository _issueRepository;

    public GetIssuesByProjectQueryHandler(IIssueRepository issueRepository)
    {
        _issueRepository = issueRepository;
    }

    public async Task<Result<PagedResult<IssueDto>>> Handle(GetIssuesByProjectQuery request, CancellationToken cancellationToken)
    {
        var query = _issueRepository.GetQueryable()
            .Where(i => i.ProjectId == request.ProjectId);

        // Filter by Status
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(i => i.Status.ToString().ToLower() == request.Status.ToLower());
        }

        // Filter by Severity
        if (!string.IsNullOrWhiteSpace(request.Severity))
        {
            query = query.Where(i => i.Severity.ToString().ToLower() == request.Severity.ToLower());
        }

        // Search in Title, Description
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(i =>
                i.Title.ToLower().Contains(searchLower) ||
                (i.Description != null && i.Description.ToLower().Contains(searchLower)));
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        // Get total count
        var totalCount = query.Count();

        // Apply pagination
        var issues = query.Skip(request.Skip).Take(request.Take).ToList();

        var dtos = issues.Select(issue => new IssueDto
        {
            Id = issue.Id,
            Title = issue.Title,
            Description = issue.Description,
            Status = issue.Status,
            Severity = issue.Severity,
            ResolvedDate = issue.ResolvedDate,
            Resolution = issue.Resolution,
            ProjectId = issue.ProjectId,
            ProjectName = issue.Project?.Name,
            ReportedById = issue.ReportedById,
            ReportedByName = issue.ReportedBy?.GetFullName(),
            AssignedToId = issue.AssignedToId,
            AssignedToName = issue.AssignedTo?.GetFullName(),
            CreatedDate = issue.CreatedAt,
            LastModifiedDate = issue.UpdatedAt
        }).ToList();

        var pagedResult = PagedResult<IssueDto>.Create(dtos, request.Page, request.PageSize, totalCount);
        return Result<PagedResult<IssueDto>>.Success(pagedResult);
    }

    private IQueryable<Domain.Entities.Issue> ApplySorting(
        IQueryable<Domain.Entities.Issue> query,
        string sortBy,
        string sortOrder)
    {
        var isDescending = sortOrder.ToLower() == "desc";

        query = sortBy.ToLower() switch
        {
            "title" => isDescending ? query.OrderByDescending(i => i.Title) : query.OrderBy(i => i.Title),
            "status" => isDescending ? query.OrderByDescending(i => i.Status) : query.OrderBy(i => i.Status),
            "severity" => isDescending ? query.OrderByDescending(i => i.Severity) : query.OrderBy(i => i.Severity),
            "createdat" => isDescending ? query.OrderByDescending(i => i.CreatedAt) : query.OrderBy(i => i.CreatedAt),
            _ => query.OrderByDescending(i => i.CreatedAt)
        };

        return query;
    }
}
