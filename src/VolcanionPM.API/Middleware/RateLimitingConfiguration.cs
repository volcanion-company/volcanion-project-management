using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace VolcanionPM.API.Middleware;

/// <summary>
/// Rate limiting configuration for API endpoints
/// Prevents abuse and DDoS attacks
/// </summary>
public static class RateLimitingConfiguration
{
    public const string FixedWindowPolicy = "fixed";
    public const string SlidingWindowPolicy = "sliding";
    public const string AuthenticationPolicy = "auth";
    
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Global rate limit: 100 requests per minute
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Fixed window: 50 requests per minute per user
            options.AddFixedWindowLimiter(policyName: FixedWindowPolicy, options =>
            {
                options.PermitLimit = 50;
                options.Window = TimeSpan.FromMinutes(1);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 2;
            });

            // Sliding window: 100 requests per minute per user (smoother limiting)
            options.AddSlidingWindowLimiter(policyName: SlidingWindowPolicy, options =>
            {
                options.PermitLimit = 100;
                options.Window = TimeSpan.FromMinutes(1);
                options.SegmentsPerWindow = 6; // 10-second segments
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 2;
            });

            // Authentication endpoints: 5 requests per minute (prevent brute force)
            options.AddFixedWindowLimiter(policyName: AuthenticationPolicy, options =>
            {
                options.PermitLimit = 5;
                options.Window = TimeSpan.FromMinutes(1);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 0; // No queueing for auth requests
            });

            // Rejection response
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
                }

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Too many requests",
                    message = "Rate limit exceeded. Please try again later.",
                    retryAfter = retryAfter.TotalSeconds
                }, cancellationToken: token);
            };
        });

        return services;
    }
}
