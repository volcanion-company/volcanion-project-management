using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Application.Features.TimeEntries.Commands.Create;

public class CreateTimeEntryCommandHandler : IRequestHandler<CreateTimeEntryCommand, Result<Guid>>
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTimeEntryCommandHandler(
        ITimeEntryRepository timeEntryRepository,
        ITaskRepository taskRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _timeEntryRepository = timeEntryRepository;
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateTimeEntryCommand request, CancellationToken cancellationToken)
    {
        // Verify task exists
        var task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task == null)
        {
            return Result<Guid>.Failure("Task not found");
        }

        // Verify user exists
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result<Guid>.Failure("User not found");
        }

        try
        {
            var timeEntry = TimeEntry.Create(
                request.UserId,
                request.TaskId,
                request.Hours,
                request.Type,
                request.Date,
                request.Description,
                request.IsBillable,
                "System");

            await _timeEntryRepository.AddAsync(timeEntry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(timeEntry.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
