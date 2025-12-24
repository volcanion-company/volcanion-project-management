using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Projects;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Projects.Queries.GetAll;

public class GetAllProjectsQueryHandler : IRequestHandler<GetAllProjectsQuery, Result<PagedResult<ProjectDto>>>
{
    private readonly IProjectRepository _projectRepository;

    public GetAllProjectsQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result<PagedResult<ProjectDto>>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
    {
        // Get base queryable
        var query = _projectRepository.GetQueryable();

        // Apply filters
        if (request.OrganizationId.HasValue)
        {
            query = query.Where(p => p.OrganizationId == request.OrganizationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ProjectStatus>(request.Status, true, out var status))
        {
            query = query.Where(p => p.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Priority) && Enum.TryParse<ProjectPriority>(request.Priority, true, out var priority))
        {
            query = query.Where(p => p.Priority == priority);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(searchLower) || 
                p.Code.ToLower().Contains(searchLower) ||
                (p.Description != null && p.Description.ToLower().Contains(searchLower)));
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        // Get total count before pagination
        var totalCount = query.Count();

        // Apply pagination and execute query
        var projects = query
            .Skip(request.Skip)
            .Take(request.Take)
            .ToList();

        // Map to DTOs
        var dtos = projects.Select(p => new ProjectDto
        {
            Id = p.Id,
            Name = p.Name,
            Code = p.Code,
            Description = p.Description,
            Status = p.Status.ToString(),
            Priority = p.Priority.ToString(),
            StartDate = p.DateRange?.StartDate,
            EndDate = p.DateRange?.EndDate,
            BudgetAmount = p.Budget?.Amount,
            BudgetCurrency = p.Budget?.Currency,
            OrganizationId = p.OrganizationId,
            CreatedAt = p.CreatedAt,
            CreatedBy = p.CreatedBy
        }).ToList();

        var pagedResult = PagedResult<ProjectDto>.Create(dtos, request.Page, request.PageSize, totalCount);

        return Result<PagedResult<ProjectDto>>.Success(pagedResult);
    }

    private static IQueryable<Domain.Entities.Project> ApplySorting(
        IQueryable<Domain.Entities.Project> query,
        string? sortBy,
        string? sortOrder)
    {
        var isDescending = sortOrder?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "code" => isDescending ? query.OrderByDescending(p => p.Code) : query.OrderBy(p => p.Code),
            "status" => isDescending ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
            "priority" => isDescending ? query.OrderByDescending(p => p.Priority) : query.OrderBy(p => p.Priority),
            "startdate" => isDescending ? query.OrderByDescending(p => p.DateRange!.StartDate) : query.OrderBy(p => p.DateRange!.StartDate),
            "enddate" => isDescending ? query.OrderByDescending(p => p.DateRange!.EndDate) : query.OrderBy(p => p.DateRange!.EndDate),
            "createdat" => isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt) // Default sorting
        };
    }
}
