using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Application.Features.Issues.Commands.Create;

public class CreateIssueCommandHandler : IRequestHandler<CreateIssueCommand, Result<Guid>>
{
    private readonly IIssueRepository _issueRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateIssueCommandHandler(
        IIssueRepository issueRepository,
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _issueRepository = issueRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateIssueCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
        {
            return Result<Guid>.Failure("Project not found");
        }

        try
        {
            var issue = Issue.Create(
                request.Title,
                request.Description,
                request.ProjectId,
                request.Severity,
                request.ReportedById,
                request.AssignedToId,
                "System");

            await _issueRepository.AddAsync(issue, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(issue.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
