using System.Text;
using System.Text.Json;

namespace VolcanionPM.API.Middleware;

/// <summary>
/// Middleware to validate and sanitize incoming request data
/// Prevents common injection attacks (XSS, SQL Injection)
/// </summary>
public class InputValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InputValidationMiddleware> _logger;

    // Dangerous patterns that should be blocked or sanitized
    private static readonly string[] DangerousPatterns = new[]
    {
        "<script", "javascript:", "onerror=", "onload=", "onclick=",
        "--", ";--", "/*", "*/", "xp_", "sp_", "exec(", "execute(",
        "drop table", "drop database", "truncate table", "union select"
    };

    public InputValidationMiddleware(RequestDelegate next, ILogger<InputValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip validation for GET requests (no body)
        if (context.Request.Method == HttpMethods.Get)
        {
            await _next(context);
            return;
        }

        // Skip validation for file uploads
        if (context.Request.ContentType?.Contains("multipart/form-data") == true)
        {
            await _next(context);
            return;
        }

        // Validate query strings
        if (context.Request.QueryString.HasValue)
        {
            var queryString = context.Request.QueryString.Value!;
            if (ContainsDangerousContent(queryString, out var pattern))
            {
                _logger.LogWarning("Potentially dangerous query string detected: {Pattern} in {QueryString}", 
                    pattern, queryString);
                
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Invalid input",
                    message = "The request contains potentially dangerous content."
                });
                return;
            }
        }

        // Validate request body (for JSON requests)
        if (context.Request.ContentType?.Contains("application/json") == true && 
            context.Request.ContentLength > 0)
        {
            context.Request.EnableBuffering();
            
            using var reader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (ContainsDangerousContent(body, out var pattern))
            {
                _logger.LogWarning("Potentially dangerous content detected in request body: {Pattern}", pattern);
                
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Invalid input",
                    message = "The request contains potentially dangerous content."
                });
                return;
            }
        }

        await _next(context);
    }

    private static bool ContainsDangerousContent(string input, out string? matchedPattern)
    {
        matchedPattern = null;
        var lowerInput = input.ToLowerInvariant();

        foreach (var pattern in DangerousPatterns)
        {
            if (lowerInput.Contains(pattern))
            {
                matchedPattern = pattern;
                return true;
            }
        }

        return false;
    }
}
