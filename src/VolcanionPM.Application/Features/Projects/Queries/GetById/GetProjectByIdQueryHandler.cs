using MediatR;
using Microsoft.Extensions.Logging;
using VolcanionPM.Application.Common.Constants;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Projects;

namespace VolcanionPM.Application.Features.Projects.Queries.GetById;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetProjectByIdQueryHandler> _logger;

    public GetProjectByIdQueryHandler(
        IProjectRepository projectRepository,
        ICacheService cacheService,
        ILogger<GetProjectByIdQueryHandler> logger)
    {
        _projectRepository = projectRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.Project(request.Id);

        _logger.LogInformation("Retrieving project {ProjectId} (cache key: {CacheKey})", request.Id, cacheKey);

        // Try cache-aside pattern
        var cachedDto = await _cacheService.GetAsync<ProjectDto>(cacheKey, cancellationToken);
        if (cachedDto != null)
        {
            _logger.LogInformation("Project {ProjectId} found in cache", request.Id);
            return Result<ProjectDto>.Success(cachedDto);
        }

        _logger.LogInformation("Project {ProjectId} not in cache, querying database", request.Id);

        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Project {ProjectId} not found", request.Id);
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

        // Store in cache for future requests
        await _cacheService.SetAsync(cacheKey, dto, CacheKeys.Expiration.Medium, cancellationToken);
        _logger.LogInformation("Project {ProjectId} cached successfully", request.Id);

        return Result<ProjectDto>.Success(dto);
    }
}
