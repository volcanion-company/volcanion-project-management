using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.TimeEntries.DTOs;

namespace VolcanionPM.Application.Features.TimeEntries.Queries.GetTimeEntriesByUser;

public class GetTimeEntriesByUserQueryHandler : IRequestHandler<GetTimeEntriesByUserQuery, Result<List<TimeEntryDto>>>
{
    private readonly ITimeEntryRepository _timeEntryRepository;

    public GetTimeEntriesByUserQueryHandler(ITimeEntryRepository timeEntryRepository)
    {
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<List<TimeEntryDto>>> Handle(GetTimeEntriesByUserQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.TimeEntry> timeEntries;

        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            timeEntries = await _timeEntryRepository.GetByDateRangeAsync(
                request.UserId,
                request.StartDate.Value,
                request.EndDate.Value,
                cancellationToken);
        }
        else
        {
            timeEntries = await _timeEntryRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        }

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
