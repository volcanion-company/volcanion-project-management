using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using VolcanionPM.API.Authorization.Requirements;
using VolcanionPM.Domain.Common;

namespace VolcanionPM.API.Authorization.Handlers;

public class IsResourceOwnerHandler : AuthorizationHandler<IsResourceOwnerRequirement, BaseEntity>
{
    private readonly ILogger<IsResourceOwnerHandler> _logger;

    public IsResourceOwnerHandler(ILogger<IsResourceOwnerHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IsResourceOwnerRequirement requirement,
        BaseEntity resource)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in claims");
            return Task.CompletedTask;
        }

        // Admins can access any resource
        if (userRole == "Admin")
        {
            _logger.LogInformation("User {UserId} authorized as Admin to access resource {ResourceId}", userId, resource.Id);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Resource owners can access their own resources
        if (resource.CreatedBy == userId)
        {
            _logger.LogInformation("User {UserId} authorized as owner to access resource {ResourceId}", userId, resource.Id);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        _logger.LogWarning("User {UserId} denied permission to access resource {ResourceId}", userId, resource.Id);
        return Task.CompletedTask;
    }
}
