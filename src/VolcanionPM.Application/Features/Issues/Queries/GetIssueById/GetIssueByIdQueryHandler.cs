using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Issues.DTOs;

namespace VolcanionPM.Application.Features.Issues.Queries.GetIssueById;

public class GetIssueByIdQueryHandler : IRequestHandler<GetIssueByIdQuery, Result<IssueDto>>
{
    private readonly IIssueRepository _issueRepository;

    public GetIssueByIdQueryHandler(IIssueRepository issueRepository)
    {
        _issueRepository = issueRepository;
    }

    public async Task<Result<IssueDto>> Handle(GetIssueByIdQuery request, CancellationToken cancellationToken)
    {
        var issue = await _issueRepository.GetByIdAsync(request.Id, cancellationToken);

        if (issue == null)
        {
            return Result<IssueDto>.Failure("Issue not found");
        }

        var dto = new IssueDto
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
        };

        return Result<IssueDto>.Success(dto);
    }
}
