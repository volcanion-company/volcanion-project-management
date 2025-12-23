using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Application.Features.Sprints.Commands.Create;

public class CreateSprintCommandHandler : IRequestHandler<CreateSprintCommand, Result<Guid>>
{
    private readonly ISprintRepository _sprintRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSprintCommandHandler(
        ISprintRepository sprintRepository,
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _sprintRepository = sprintRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateSprintCommand request, CancellationToken cancellationToken)
    {
        // Verify project exists
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
        {
            return Result<Guid>.Failure("Project not found");
        }

        var dateRange = DateRange.Create(request.StartDate, request.EndDate);

        var sprint = Sprint.Create(
            request.Name,
            request.SprintNumber,
            request.ProjectId,
            dateRange,
            request.Goal,
            request.TotalStoryPoints,
            "System");

        await _sprintRepository.AddAsync(sprint, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(sprint.Id);
    }
}
