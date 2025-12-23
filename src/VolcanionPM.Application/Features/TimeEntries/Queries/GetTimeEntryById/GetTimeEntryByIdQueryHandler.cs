using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.TimeEntries.DTOs;

namespace VolcanionPM.Application.Features.TimeEntries.Queries.GetTimeEntryById;

public class GetTimeEntryByIdQueryHandler : IRequestHandler<GetTimeEntryByIdQuery, Result<TimeEntryDto>>
{
    private readonly ITimeEntryRepository _timeEntryRepository;

    public GetTimeEntryByIdQueryHandler(ITimeEntryRepository timeEntryRepository)
    {
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<TimeEntryDto>> Handle(GetTimeEntryByIdQuery request, CancellationToken cancellationToken)
    {
        var timeEntry = await _timeEntryRepository.GetByIdAsync(request.Id, cancellationToken);

        if (timeEntry == null)
        {
            return Result<TimeEntryDto>.Failure("Time entry not found");
        }

        var dto = new TimeEntryDto
        {
            Id = timeEntry.Id,
            UserId = timeEntry.UserId,
            UserName = timeEntry.User?.GetFullName(),
            TaskId = timeEntry.TaskId,
            TaskTitle = timeEntry.Task?.Title,
            Hours = timeEntry.Hours,
            Type = timeEntry.Type,
            Date = timeEntry.Date,
            Description = timeEntry.Description,
            IsBillable = timeEntry.IsBillable,
            CreatedDate = timeEntry.CreatedAt,
            LastModifiedDate = timeEntry.UpdatedAt
        };

        return Result<TimeEntryDto>.Success(dto);
    }
}
