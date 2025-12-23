using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Projects.Commands.Delete;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Result<bool>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProjectCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        // Get existing project
        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (project == null)
        {
            return Result<bool>.Failure($"Project with ID {request.Id} not found");
        }

        // Remove project (hard delete for now - soft delete would require domain support)
        _projectRepository.Remove(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
