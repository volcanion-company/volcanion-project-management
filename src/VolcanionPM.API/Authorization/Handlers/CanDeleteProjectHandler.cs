using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using VolcanionPM.API.Authorization.Requirements;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.API.Authorization.Handlers;

public class CanDeleteProjectHandler : AuthorizationHandler<CanDeleteProjectRequirement, Project>
{
    private readonly ILogger<CanDeleteProjectHandler> _logger;

    public CanDeleteProjectHandler(ILogger<CanDeleteProjectHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanDeleteProjectRequirement requirement,
        Project resource)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in claims");
            return Task.CompletedTask;
        }

        // Only Admins can delete projects
        if (userRole == "Admin")
        {
            _logger.LogInformation("User {UserId} authorized as Admin to delete project {ProjectId}", userId, resource.Id);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        _logger.LogWarning("User {UserId} denied permission to delete project {ProjectId}", userId, resource.Id);
        return Task.CompletedTask;
    }
}
