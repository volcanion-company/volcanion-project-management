using System.Diagnostics;
using System.Text;

namespace VolcanionPM.API.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for health check and metrics endpoints
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/metrics"))
        {
            await _next(context);
            return;
        }

        var correlationId = context.Items["CorrelationId"]?.ToString();
        var stopwatch = Stopwatch.StartNew();

        // Log request
        await LogRequest(context, correlationId);

        // Capture original response body stream
        var originalBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Execute the request
            await _next(context);

            stopwatch.Stop();

            // Log response
            await LogResponse(context, correlationId, stopwatch.ElapsedMilliseconds);

            // Copy response body to original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogRequest(HttpContext context, string? correlationId)
    {
        var request = context.Request;

        var requestLog = new
        {
            CorrelationId = correlationId,
            Method = request.Method,
            Path = request.Path.ToString(),
            QueryString = request.QueryString.ToString(),
            Headers = GetSafeHeaders(request.Headers),
            Body = await GetRequestBody(request)
        };

        _logger.LogInformation(
            "HTTP Request: {Method} {Path}{QueryString} - CorrelationId: {CorrelationId}",
            request.Method,
            request.Path,
            request.QueryString,
            correlationId);
    }

    private async Task LogResponse(HttpContext context, string? correlationId, long elapsedMs)
    {
        var response = context.Response;

        response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        var responseLog = new
        {
            CorrelationId = correlationId,
            StatusCode = response.StatusCode,
            ElapsedMs = elapsedMs,
            Headers = GetSafeHeaders(response.Headers),
            BodySize = responseBody.Length
        };

        var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
        _logger.Log(logLevel, "HTTP Response: {StatusCode} - {ElapsedMs}ms - CorrelationId: {CorrelationId}", response.StatusCode, elapsedMs, correlationId);
    }

    private async Task<string> GetRequestBody(HttpRequest request)
    {
        if (request.ContentLength == null || request.ContentLength == 0)
            return string.Empty;

        // Only log text-based content types
        if (!request.ContentType?.Contains("application/json") == true &&
            !request.ContentType?.Contains("application/xml") == true &&
            !request.ContentType?.Contains("text/") == true)
        {
            return $"[Binary content: {request.ContentType}]";
        }

        request.EnableBuffering();
        request.Body.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        request.Body.Seek(0, SeekOrigin.Begin);

        // Mask sensitive data (passwords, tokens, etc.)
        body = MaskSensitiveData(body);

        return body.Length > 1000 ? body.Substring(0, 1000) + "..." : body;
    }

    private Dictionary<string, string> GetSafeHeaders(IHeaderDictionary headers)
    {
        var safeHeaders = new Dictionary<string, string>();
        var sensitiveHeaders = new[] { "authorization", "cookie", "set-cookie", "x-api-key" };

        foreach (var header in headers)
        {
            var key = header.Key.ToLower();
            if (sensitiveHeaders.Contains(key))
            {
                safeHeaders[header.Key] = "***REDACTED***";
            }
            else
            {
                safeHeaders[header.Key] = header.Value.ToString();
            }
        }

        return safeHeaders;
    }

    private string MaskSensitiveData(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return body;

        // Simple regex-based masking for common sensitive fields
        var sensitiveFields = new[] { "password", "token", "secret", "apikey", "api_key" };

        foreach (var field in sensitiveFields)
        {
            // Match JSON property: "field": "value"
            body = System.Text.RegularExpressions.Regex.Replace(
                body,
                $"\"{field}\"\\s*:\\s*\"[^\"]*\"",
                $"\"{field}\": \"***REDACTED***\"",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return body;
    }
}
