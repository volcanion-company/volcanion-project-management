using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using VolcanionPM.API.Authorization.Requirements;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Application.Common.Interfaces;

namespace VolcanionPM.API.Authorization.Handlers;

public class CanEditProjectHandler : AuthorizationHandler<CanEditProjectRequirement, Project>
{
    private readonly ILogger<CanEditProjectHandler> _logger;

    public CanEditProjectHandler(ILogger<CanEditProjectHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanEditProjectRequirement requirement,
        Project resource)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in claims");
            return Task.CompletedTask;
        }

        // Admins can edit any project
        if (userRole == "Admin")
        {
            _logger.LogInformation("User {UserId} authorized as Admin to edit project {ProjectId}", userId, resource.Id);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Project managers can edit their own projects
        if (resource.ProjectManagerId == Guid.Parse(userId))
        {
            _logger.LogInformation("User {UserId} authorized as ProjectManager to edit project {ProjectId}", userId, resource.Id);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        _logger.LogWarning("User {UserId} denied permission to edit project {ProjectId}", userId, resource.Id);
        return Task.CompletedTask;
    }
}
