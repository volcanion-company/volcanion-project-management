using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.TimeEntries.DTOs;

namespace VolcanionPM.Application.Features.TimeEntries.Queries.GetTimeEntriesByUser;

public class GetTimeEntriesByUserQueryHandler : IRequestHandler<GetTimeEntriesByUserQuery, Result<PagedResult<TimeEntryDto>>>
{
    private readonly ITimeEntryRepository _timeEntryRepository;

    public GetTimeEntriesByUserQueryHandler(ITimeEntryRepository timeEntryRepository)
    {
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<Result<PagedResult<TimeEntryDto>>> Handle(GetTimeEntriesByUserQuery request, CancellationToken cancellationToken)
    {
        var query = _timeEntryRepository.GetQueryable()
            .Where(te => te.UserId == request.UserId);

        // Filter by TaskId
        if (request.TaskId.HasValue)
        {
            query = query.Where(te => te.TaskId == request.TaskId.Value);
        }

        // Filter by date range
        if (request.StartDate.HasValue)
        {
            query = query.Where(te => te.Date >= request.StartDate.Value);
        }
        if (request.EndDate.HasValue)
        {
            query = query.Where(te => te.Date <= request.EndDate.Value);
        }

        // Filter by Type
        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            query = query.Where(te => te.Type.ToString().ToLower() == request.Type.ToLower());
        }

        // Filter by IsBillable
        if (request.IsBillable.HasValue)
        {
            query = query.Where(te => te.IsBillable == request.IsBillable.Value);
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        // Get total count
        var totalCount = query.Count();

        // Apply pagination
        var timeEntries = query.Skip(request.Skip).Take(request.Take).ToList();

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

        var pagedResult = PagedResult<TimeEntryDto>.Create(dtos, request.Page, request.PageSize, totalCount);
        return Result<PagedResult<TimeEntryDto>>.Success(pagedResult);
    }

    private IQueryable<Domain.Entities.TimeEntry> ApplySorting(
        IQueryable<Domain.Entities.TimeEntry> query,
        string sortBy,
        string sortOrder)
    {
        var isDescending = sortOrder.ToLower() == "desc";

        query = sortBy.ToLower() switch
        {
            "date" => isDescending ? query.OrderByDescending(te => te.Date) : query.OrderBy(te => te.Date),
            "hours" => isDescending ? query.OrderByDescending(te => te.Hours) : query.OrderBy(te => te.Hours),
            "type" => isDescending ? query.OrderByDescending(te => te.Type) : query.OrderBy(te => te.Type),
            "isbillable" => isDescending ? query.OrderByDescending(te => te.IsBillable) : query.OrderBy(te => te.IsBillable),
            "createdat" => isDescending ? query.OrderByDescending(te => te.CreatedAt) : query.OrderBy(te => te.CreatedAt),
            _ => query.OrderByDescending(te => te.Date)
        };

        return query;
    }
}
