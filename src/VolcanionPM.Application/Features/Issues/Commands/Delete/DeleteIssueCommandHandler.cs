using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Issues.Commands.Delete;

public class DeleteIssueCommandHandler : IRequestHandler<DeleteIssueCommand, Result<Unit>>
{
    private readonly IIssueRepository _issueRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteIssueCommandHandler(
        IIssueRepository issueRepository,
        IUnitOfWork unitOfWork)
    {
        _issueRepository = issueRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(DeleteIssueCommand request, CancellationToken cancellationToken)
    {
        var issue = await _issueRepository.GetByIdAsync(request.Id, cancellationToken);

        if (issue == null)
        {
            return Result<Unit>.Failure("Issue not found");
        }

        issue.MarkAsDeleted("System");
        _issueRepository.Update(issue);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
