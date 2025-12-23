using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Application.Features.Sprints.Commands.Update;

public class UpdateSprintCommandHandler : IRequestHandler<UpdateSprintCommand, Result<Unit>>
{
    private readonly ISprintRepository _sprintRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSprintCommandHandler(
        ISprintRepository sprintRepository,
        IUnitOfWork unitOfWork)
    {
        _sprintRepository = sprintRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(UpdateSprintCommand request, CancellationToken cancellationToken)
    {
        var sprint = await _sprintRepository.GetByIdAsync(request.Id, cancellationToken);

        if (sprint == null)
        {
            return Result<Unit>.Failure("Sprint not found");
        }

        try
        {
            var dateRange = DateRange.Create(request.StartDate, request.EndDate);

            sprint.UpdateDetails(
                request.Name,
                request.Goal,
                dateRange,
                request.TotalStoryPoints,
                "System");

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
