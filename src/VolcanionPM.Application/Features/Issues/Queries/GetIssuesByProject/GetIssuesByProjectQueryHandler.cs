using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Issues.DTOs;

namespace VolcanionPM.Application.Features.Issues.Queries.GetIssuesByProject;

public class GetIssuesByProjectQueryHandler : IRequestHandler<GetIssuesByProjectQuery, Result<List<IssueDto>>>
{
    private readonly IIssueRepository _issueRepository;

    public GetIssuesByProjectQueryHandler(IIssueRepository issueRepository)
    {
        _issueRepository = issueRepository;
    }

    public async Task<Result<List<IssueDto>>> Handle(GetIssuesByProjectQuery request, CancellationToken cancellationToken)
    {
        var issues = await _issueRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);

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

        return Result<List<IssueDto>>.Success(dtos);
    }
}
