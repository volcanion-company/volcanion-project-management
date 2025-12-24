using MediatR;
using Microsoft.Extensions.Logging;
using VolcanionPM.Application.Common.Constants;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Features.Projects.Commands.Create;
using VolcanionPM.Application.Features.Projects.Commands.Update;
using VolcanionPM.Application.Features.Projects.Commands.Delete;
using VolcanionPM.Application.Features.Users.Commands.Create;
using VolcanionPM.Application.Features.Users.Commands.Update;
using VolcanionPM.Application.Features.Users.Commands.Delete;

namespace VolcanionPM.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that automatically invalidates cache entries when entities are modified
/// </summary>
public class CacheInvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheInvalidationBehavior<TRequest, TResponse>> _logger;

    public CacheInvalidationBehavior(
        ICacheService cacheService,
        ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Execute the request first
        var response = await next();

        // Only invalidate cache for command operations (Create, Update, Delete)
        // Skip queries and failed operations
        if (!IsSuccessfulCommand(response))
            return response;

        // Invalidate cache based on request type
        await InvalidateCacheForRequest(request, cancellationToken);

        return response;
    }

    private bool IsSuccessfulCommand(TResponse response)
    {
        // Check if this is a command (not a query)
        var requestType = typeof(TRequest).Name;
        if (!requestType.Contains("Command"))
            return false;

        // Check if the response indicates success
        // Assuming Result<T> pattern with IsSuccess property
        var responseType = response?.GetType();
        var isSuccessProperty = responseType?.GetProperty("IsSuccess");
        
        if (isSuccessProperty != null)
        {
            var isSuccess = (bool?)isSuccessProperty.GetValue(response);
            return isSuccess == true;
        }

        return true; // Assume success if we can't determine
    }

    private async Task InvalidateCacheForRequest(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Project commands
            if (request is CreateProjectCommand createProject)
            {
                _logger.LogInformation("Invalidating project cache after create");
                await _cacheService.RemoveByPatternAsync(CacheKeys.ProjectPattern(), cancellationToken);
            }
            else if (request is UpdateProjectCommand updateProject)
            {
                _logger.LogInformation("Invalidating cache for project {ProjectId}", updateProject.Id);
                await _cacheService.RemoveAsync(CacheKeys.Project(updateProject.Id), cancellationToken);
                await _cacheService.RemoveByPatternAsync(CacheKeys.ProjectPattern(), cancellationToken);
            }
            else if (request is DeleteProjectCommand deleteProject)
            {
                _logger.LogInformation("Invalidating cache for deleted project {ProjectId}", deleteProject.Id);
                await _cacheService.RemoveAsync(CacheKeys.Project(deleteProject.Id), cancellationToken);
                await _cacheService.RemoveByPatternAsync(CacheKeys.ProjectPattern(), cancellationToken);
            }
            // User commands
            else if (request is CreateUserCommand)
            {
                _logger.LogInformation("Invalidating user cache after create");
                await _cacheService.RemoveByPatternAsync(CacheKeys.UserPattern(), cancellationToken);
            }
            else if (request is UpdateUserCommand updateUser)
            {
                _logger.LogInformation("Invalidating cache for user {UserId}", updateUser.Id);
                await _cacheService.RemoveAsync(CacheKeys.User(updateUser.Id), cancellationToken);
                await _cacheService.RemoveByPatternAsync(CacheKeys.UserPattern(), cancellationToken);
            }
            else if (request is DeleteUserCommand deleteUser)
            {
                _logger.LogInformation("Invalidating cache for deleted user {UserId}", deleteUser.Id);
                await _cacheService.RemoveAsync(CacheKeys.User(deleteUser.Id), cancellationToken);
                await _cacheService.RemoveByPatternAsync(CacheKeys.UserPattern(), cancellationToken);
            }
            // Add more command types as needed
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache invalidation failed for {RequestType}", typeof(TRequest).Name);
            // Don't throw - cache invalidation failure shouldn't break the operation
        }
    }
}
