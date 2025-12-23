using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Sprints.Commands.CompleteSprint;

public class CompleteSprintCommandHandler : IRequestHandler<CompleteSprintCommand, Result<Unit>>
{
    private readonly ISprintRepository _sprintRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteSprintCommandHandler(
        ISprintRepository sprintRepository,
        IUnitOfWork unitOfWork)
    {
        _sprintRepository = sprintRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(CompleteSprintCommand request, CancellationToken cancellationToken)
    {
        var sprint = await _sprintRepository.GetSprintWithTasksAsync(request.Id, cancellationToken);

        if (sprint == null)
        {
            return Result<Unit>.Failure("Sprint not found");
        }

        try
        {
            sprint.Complete("System");
            _sprintRepository.Update(sprint);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (InvalidOperationException ex)
        {
            return Result<Unit>.Failure(ex.Message);
        }
    }
}
