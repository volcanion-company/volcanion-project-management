using MediatR;
using Microsoft.Extensions.Logging;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Projects;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Application.Features.Projects.Commands.Create;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<ProjectDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProjectCommandHandler> _logger;

    public CreateProjectCommandHandler(
        IProjectRepository projectRepository, 
        IUnitOfWork unitOfWork,
        ILogger<CreateProjectCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating project with code {ProjectCode} for organization {OrganizationId}", 
            request.Code, request.OrganizationId);

        // Check if code already exists
        var existingProject = await _projectRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existingProject != null)
        {
            _logger.LogWarning("Project creation failed: Code {ProjectCode} already exists", request.Code);
            return Result<ProjectDto>.Failure($"Project with code '{request.Code}' already exists");
        }

        // Parse priority
        if (!Enum.TryParse<ProjectPriority>(request.Priority, true, out var priority))
        {
            return Result<ProjectDto>.Failure($"Invalid priority: {request.Priority}");
        }

        // Create budget if provided
        Money? budget = null;
        if (request.BudgetAmount.HasValue && !string.IsNullOrEmpty(request.BudgetCurrency))
        {
            budget = Money.Create(request.BudgetAmount.Value, request.BudgetCurrency);
        }
        else
        {
            budget = Money.Create(0, "USD"); // Default budget
        }

        // Create date range if provided
        DateRange? dateRange = null;
        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            dateRange = DateRange.Create(request.StartDate.Value, request.EndDate.Value);
        }
        else
        {
            // Default date range (start today, end in 90 days)
            dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(90));
        }

        // Create project - note: missing projectManagerId, using organizationId as placeholder
        var project = Project.Create(
            request.Name,
            request.Code,
            request.OrganizationId,
            request.OrganizationId, // TODO: Add projectManagerId to command
            dateRange,
            budget,
            priority,
            request.Description,
            request.CreatedBy
        );

        await _projectRepository.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Project created successfully: {ProjectId} - {ProjectName} ({ProjectCode})",
            project.Id, project.Name, project.Code);

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
            OrganizationId = request.OrganizationId,
            CreatedAt = project.CreatedAt,
            CreatedBy = project.CreatedBy
        };

        return Result<ProjectDto>.Success(dto);
    }
}
