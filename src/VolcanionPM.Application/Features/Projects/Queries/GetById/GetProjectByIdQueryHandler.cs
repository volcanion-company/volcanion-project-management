using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Projects;

namespace VolcanionPM.Application.Features.Projects.Queries.GetById;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectByIdQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);

        if (project == null)
        {
            return Result<ProjectDto>.Failure($"Project with ID {request.Id} not found");
        }

        var dto = new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Code = project.Code,
            Description = project.Description,
            Status = project.Status.ToString(),
            Priority = project.Priority.ToString(),
            StartDate = project.DateRange?.StartDate,
            EndDate = project.DateRange?.EndDate,
            BudgetAmount = project.Budget?.Amount,
            BudgetCurrency = project.Budget?.Currency,
            OrganizationId = project.Id, // Note: This should be from shadow property in real implementation
            CreatedAt = project.CreatedAt,
            CreatedBy = project.CreatedBy
        };

        return Result<ProjectDto>.Success(dto);
    }
}
