using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.TimeEntries.Commands.Update;

public class UpdateTimeEntryCommandHandler : IRequestHandler<UpdateTimeEntryCommand, Result<Unit>>
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTimeEntryCommandHandler(
        ITimeEntryRepository timeEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _timeEntryRepository = timeEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(UpdateTimeEntryCommand request, CancellationToken cancellationToken)
    {
        var timeEntry = await _timeEntryRepository.GetByIdAsync(request.Id, cancellationToken);

        if (timeEntry == null)
        {
            return Result<Unit>.Failure("Time entry not found");
        }

        try
        {
            timeEntry.Update(
                request.Hours,
                request.Type,
                request.Date,
                request.Description,
                request.IsBillable,
                "System");

            _timeEntryRepository.Update(timeEntry);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (ArgumentException ex)
        {
            return Result<Unit>.Failure(ex.Message);
        }
    }
}
