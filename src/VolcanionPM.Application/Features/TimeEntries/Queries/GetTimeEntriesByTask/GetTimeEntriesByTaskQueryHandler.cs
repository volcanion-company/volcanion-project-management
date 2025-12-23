using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.TimeEntries.DTOs;

namespace VolcanionPM.Application.Features.TimeEntries.Queries.GetTimeEntriesByTask;

public class GetTimeEntriesByTaskQueryHandler : IRequestHandler<GetTimeEntriesByTaskQuery, Result<List<TimeEntryDto>>>
{
    private readonly ITimeEntryRepository _timeEntryRepository;

    public GetTimeEntriesByTaskQueryHandler(ITimeEntryRepository timeEntryRepository)
    {
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<List<TimeEntryDto>>> Handle(GetTimeEntriesByTaskQuery request, CancellationToken cancellationToken)
    {
        var timeEntries = await _timeEntryRepository.GetByTaskIdAsync(request.TaskId, cancellationToken);

        var dtos = timeEntries.Select(te => new TimeEntryDto
        {
            Id = te.Id,
            UserId = te.UserId,
            UserName = te.User?.GetFullName(),
            TaskId = te.TaskId,
            TaskTitle = te.Task?.Title,
            Hours = te.Hours,
            Type = te.Type,
            Date = te.Date,
            Description = te.Description,
            IsBillable = te.IsBillable,
            CreatedDate = te.CreatedAt,
            LastModifiedDate = te.UpdatedAt
        }).ToList();

        return Result<List<TimeEntryDto>>.Success(dtos);
    }
}
