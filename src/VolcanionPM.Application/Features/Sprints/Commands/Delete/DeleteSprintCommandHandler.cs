using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Sprints.Commands.Delete;

public class DeleteSprintCommandHandler : IRequestHandler<DeleteSprintCommand, Result<Unit>>
{
    private readonly ISprintRepository _sprintRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSprintCommandHandler(
        ISprintRepository sprintRepository,
        IUnitOfWork unitOfWork)
    {
        _sprintRepository = sprintRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(DeleteSprintCommand request, CancellationToken cancellationToken)
    {
        var sprint = await _sprintRepository.GetByIdAsync(request.Id, cancellationToken);

        if (sprint == null)
        {
            return Result<Unit>.Failure("Sprint not found");
        }

        // Soft delete
        sprint.MarkAsDeleted("System");
        _sprintRepository.Update(sprint);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
