using System.Net;
using System.Text.Json;

namespace VolcanionPM.API.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
        
        _logger.LogError(exception, 
            "An unhandled exception occurred. CorrelationId: {CorrelationId}", 
            correlationId);

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            CorrelationId = correlationId,
            Message = "An error occurred while processing your request."
        };

        switch (exception)
        {
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = "Unauthorized access.";
                break;
            case ArgumentNullException:
            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = exception.Message;
                break;
            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = "Resource not found.";
                break;
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse);
        await response.WriteAsync(result);
    }

    private class ErrorResponse
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
