using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.TimeEntries.Commands.Delete;

public class DeleteTimeEntryCommandHandler : IRequestHandler<DeleteTimeEntryCommand, Result<Unit>>
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTimeEntryCommandHandler(
        ITimeEntryRepository timeEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _timeEntryRepository = timeEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(DeleteTimeEntryCommand request, CancellationToken cancellationToken)
    {
        var timeEntry = await _timeEntryRepository.GetByIdAsync(request.Id, cancellationToken);

        if (timeEntry == null)
        {
            return Result<Unit>.Failure("Time entry not found");
        }

        timeEntry.MarkAsDeleted("System");
        _timeEntryRepository.Update(timeEntry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
