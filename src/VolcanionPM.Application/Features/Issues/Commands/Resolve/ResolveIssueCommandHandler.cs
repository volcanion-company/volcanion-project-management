using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Issues.Commands.Resolve;

public class ResolveIssueCommandHandler : IRequestHandler<ResolveIssueCommand, Result<Unit>>
{
    private readonly IIssueRepository _issueRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResolveIssueCommandHandler(
        IIssueRepository issueRepository,
        IUnitOfWork unitOfWork)
    {
        _issueRepository = issueRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(ResolveIssueCommand request, CancellationToken cancellationToken)
    {
        var issue = await _issueRepository.GetByIdAsync(request.Id, cancellationToken);

        if (issue == null)
        {
            return Result<Unit>.Failure("Issue not found");
        }

        issue.Resolve(request.Resolution, "System");
        _issueRepository.Update(issue);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
