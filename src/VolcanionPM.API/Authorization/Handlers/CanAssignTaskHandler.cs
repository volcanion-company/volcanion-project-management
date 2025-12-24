using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using VolcanionPM.API.Authorization.Requirements;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Application.Common.Interfaces;

namespace VolcanionPM.API.Authorization.Handlers;

public class CanAssignTaskHandler : AuthorizationHandler<CanAssignTaskRequirement, ProjectTask>
{
    private readonly ILogger<CanAssignTaskHandler> _logger;
    private readonly IProjectRepository _projectRepository;

    public CanAssignTaskHandler(
        ILogger<CanAssignTaskHandler> logger,
        IProjectRepository projectRepository)
    {
        _logger = logger;
        _projectRepository = projectRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanAssignTaskRequirement requirement,
        ProjectTask resource)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in claims");
            return;
        }

        // Admins can assign any task
        if (userRole == "Admin")
        {
            _logger.LogInformation("User {UserId} authorized as Admin to assign task {TaskId}", userId, resource.Id);
            context.Succeed(requirement);
            return;
        }

        // Get the project to check if user is the project manager
        var project = await _projectRepository.GetByIdAsync(resource.ProjectId, CancellationToken.None);
        
        if (project != null && project.ProjectManagerId == Guid.Parse(userId))
        {
            _logger.LogInformation("User {UserId} authorized as ProjectManager to assign task {TaskId}", userId, resource.Id);
            context.Succeed(requirement);
            return;
        }

        _logger.LogWarning("User {UserId} denied permission to assign task {TaskId}", userId, resource.Id);
    }
}
