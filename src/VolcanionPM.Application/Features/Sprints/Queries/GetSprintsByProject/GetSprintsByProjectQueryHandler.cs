using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Sprints.DTOs;

namespace VolcanionPM.Application.Features.Sprints.Queries.GetSprintsByProject;

public class GetSprintsByProjectQueryHandler : IRequestHandler<GetSprintsByProjectQuery, Result<PagedResult<SprintDto>>>
{
    private readonly ISprintRepository _sprintRepository;
    private readonly IProjectRepository _projectRepository;

    public GetSprintsByProjectQueryHandler(
        ISprintRepository sprintRepository,
        IProjectRepository projectRepository)
    {
        _sprintRepository = sprintRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Result<PagedResult<SprintDto>>> Handle(GetSprintsByProjectQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
        {
            return Result<PagedResult<SprintDto>>.Failure("Project not found");
        }

        var query = _sprintRepository.GetQueryable()
            .Where(s => s.ProjectId == request.ProjectId);

        // Filter by Status
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(s => s.Status.ToString().ToLower() == request.Status.ToLower());
        }

        // Search in Name, Goal
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(searchLower) ||
                (s.Goal != null && s.Goal.ToLower().Contains(searchLower)));
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        // Get total count
        var totalCount = query.Count();

        // Apply pagination
        var sprints = query.Skip(request.Skip).Take(request.Take).ToList();

        var sprintDtos = sprints.Select(sprint => new SprintDto
        {
            Id = sprint.Id,
            Name = sprint.Name,
            Goal = sprint.Goal,
            SprintNumber = sprint.SprintNumber,
            StartDate = sprint.DateRange.StartDate,
            EndDate = sprint.DateRange.EndDate,
            Status = sprint.Status,
            TotalStoryPoints = sprint.TotalStoryPoints,
            CompletedStoryPoints = sprint.CompletedStoryPoints,
            ProjectId = sprint.ProjectId,
            ProjectName = project.Name,
            TaskCount = sprint.Tasks.Count,
            CompletedTaskCount = sprint.GetCompletedTaskCount(),
            CreatedDate = sprint.CreatedAt,
            LastModifiedDate = sprint.UpdatedAt
        }).ToList();

        var pagedResult = PagedResult<SprintDto>.Create(sprintDtos, request.Page, request.PageSize, totalCount);
        return Result<PagedResult<SprintDto>>.Success(pagedResult);
    }

    private IQueryable<Domain.Entities.Sprint> ApplySorting(
        IQueryable<Domain.Entities.Sprint> query,
        string sortBy,
        string sortOrder)
    {
        var isDescending = sortOrder.ToLower() == "desc";

        query = sortBy.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
            "sprintnumber" => isDescending ? query.OrderByDescending(s => s.SprintNumber) : query.OrderBy(s => s.SprintNumber),
            "startdate" => isDescending ? query.OrderByDescending(s => s.DateRange.StartDate) : query.OrderBy(s => s.DateRange.StartDate),
            "enddate" => isDescending ? query.OrderByDescending(s => s.DateRange.EndDate) : query.OrderBy(s => s.DateRange.EndDate),
            "status" => isDescending ? query.OrderByDescending(s => s.Status) : query.OrderBy(s => s.Status),
            "createdat" => isDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            _ => query.OrderByDescending(s => s.SprintNumber)
        };

        return query;
    }
}
