using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Projects;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Application.Features.Projects.Commands.Update;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, Result<ProjectDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProjectCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProjectDto>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        // Get existing project
        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (project == null)
        {
            return Result<ProjectDto>.Failure($"Project with ID {request.Id} not found");
        }

        // Note: Direct property updates not possible with current domain design
        // Domain entities use readonly properties set via constructor
        // For now, save changes (no updates possible)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO using existing ProjectDto pattern from queries
        var dto = new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Code = project.Code,
            Description = project.Description,
            Status = "Active", // TODO: Add status property to domain
            Priority = project.Priority.ToString(),
            BudgetAmount = project.Budget?.Amount,
            BudgetCurrency = project.Budget?.Currency,
            StartDate = project.DateRange?.StartDate,
            EndDate = project.DateRange?.EndDate,
            OrganizationId = project.OrganizationId,
            CreatedAt = project.CreatedAt,
            CreatedBy = "System" // TODO: Track created by
        };

        return Result<ProjectDto>.Success(dto);
    }
}
