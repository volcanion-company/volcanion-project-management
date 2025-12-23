using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Projects;

namespace VolcanionPM.Application.Features.Projects.Queries.GetAll;

public class GetAllProjectsQueryHandler : IRequestHandler<GetAllProjectsQuery, Result<List<ProjectDto>>>
{
    private readonly IProjectRepository _projectRepository;

    public GetAllProjectsQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result<List<ProjectDto>>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
    {
        var projects = request.OrganizationId.HasValue
            ? await _projectRepository.GetByOrganizationIdAsync(request.OrganizationId.Value, cancellationToken)
            : await _projectRepository.GetAllAsync(cancellationToken);

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
            OrganizationId = p.Id,
            CreatedAt = p.CreatedAt,
            CreatedBy = p.CreatedBy
        }).ToList();

        return Result<List<ProjectDto>>.Success(dtos);
    }
}
