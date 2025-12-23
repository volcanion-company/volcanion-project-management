using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Issues.Commands.Update;

public class UpdateIssueCommandHandler : IRequestHandler<UpdateIssueCommand, Result<Unit>>
{
    private readonly IIssueRepository _issueRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateIssueCommandHandler(
        IIssueRepository issueRepository,
        IUnitOfWork unitOfWork)
    {
        _issueRepository = issueRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(UpdateIssueCommand request, CancellationToken cancellationToken)
    {
        var issue = await _issueRepository.GetByIdAsync(request.Id, cancellationToken);

        if (issue == null)
        {
            return Result<Unit>.Failure("Issue not found");
        }

        try
        {
            issue.Update(
                request.Title,
                request.Description,
                request.Severity,
                "System");

            _issueRepository.Update(issue);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (ArgumentException ex)
        {
            return Result<Unit>.Failure(ex.Message);
        }
    }
}
